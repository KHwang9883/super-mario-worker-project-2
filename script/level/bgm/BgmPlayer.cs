using Godot;
using System;
using System.IO;
using SMWP.Level;
using SMWP.Level.Player;
using SMWP.Level.Tool;

public partial class BgmPlayer : AudioStreamPlayer {
    [Export] private AudioStreamPlayer _bgm146Player = null!;
    private Node2D? _player;
    private PlayerDieAndHurt? _playerDieAndHurt;
    private PlayerSuit? _playerSuit;
    private LevelConfig? _levelConfig;
    private bool _fadeOut;

    private float _onWaterVolume;
    private float _underWaterVolume;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        
        Callable.From(() => {
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            _playerDieAndHurt = (PlayerDieAndHurt)_player.GetMeta("PlayerDieAndHurt");
            _playerDieAndHurt.PlayerDiedSucceeded += OnPlayerDied;
            _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
            _playerSuit.PlayerStarmanStarted += OnPlayerStarmanStart;
            _playerSuit.PlayerStarmanFinished += OnPlayerStarmanEnd;
            
            // Fast Retry 读取 BGM 位置
            if (_levelConfig.BgmId != LevelManager.CurrentBgmId) {
                LevelManager.BgmPosition = 0f;
            }
            SetBgm(false);
            Play(_levelConfig.FastRetry ? LevelManager.BgmPosition : 0f);
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: Level Config is null!");
            return;
        }
        
        // 过关与 Fast Retry 处理
        if (LevelManager.IsLevelPass && Playing) {
            Stop();
        }
        if (_fadeOut && _levelConfig.BgmId != 146) {
            VolumeLinear = Mathf.MoveToward(VolumeLinear, 0.2f, 0.15f);
            return;
        }
        
        // 第 146 号 BGM 音频处理
        if (_levelConfig.BgmId != 146) {
            VolumeLinear = Mathf.MoveToward(VolumeLinear, 1f, 0.05f);
            _bgm146Player.Stop();
            return;
        }
        
        // 音频播放状态同步
        Bgm146Sync();
        
        if (_player == null) {
            // 出于顺序问题在 _Ready() 中延迟引用的 Player 不及时
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            if (_player == null)
                GD.PushError($"{this}: Player is null!");
        } else if (!_fadeOut) {
            var playerMovement = (PlayerMovement)_player.GetMeta("PlayerMovement");
            switch (playerMovement.IsAroundWater) {
                case true when !playerMovement.IsInWater:
                    _onWaterVolume = 0.5f;
                    _underWaterVolume = 0.5f;
                    break;
                case false:
                    _onWaterVolume = 1f;
                    _underWaterVolume = 0f;
                    break;
                default: {
                    if (playerMovement.IsInWater) {
                        _onWaterVolume = 0f;
                        _underWaterVolume = 1f;
                    }
                    break;
                }
            }
            VolumeLinear = Mathf.MoveToward(VolumeLinear, _onWaterVolume, 0.1f);
            _bgm146Player.VolumeLinear = Mathf.MoveToward(_bgm146Player.VolumeLinear, _underWaterVolume, 0.1f);
        }
    }

    public void SetBgm(bool play) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            foreach (var entry in _levelConfig.BgmDatabase.Entries) {
                if (entry.BgmId != _levelConfig.BgmId) continue;
                LevelManager.CurrentBgmId = _levelConfig.BgmId;
            
                // 首先获取外置的覆盖 BGM
                foreach (var fileName in entry.FileNameForOverride) {
                    // 遍历所有的兼容性文件名
                    var baseDir = Path.GetDirectoryName(OS.GetExecutablePath());
                    baseDir = baseDir?.Replace("\\", "/");
                    var bgmPath =
                        baseDir + "/Data/" + entry.AlbumPath + "/" + fileName;
                    // 因为之前 SMWP 版本后缀过于混乱，无法进行识别，故不保留文件后缀
                    bgmPath = bgmPath.GetBaseName();
                    
                    // 猜测 BGM 格式以及真实格式
                    switch (BgmFileFormatGuess.GetGuessFormat(bgmPath)) {
                        case BgmFileFormatGuess.BgmFileTypeEnum.Mp3:
                            Stream = AudioStreamMP3.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                            break;
                        case BgmFileFormatGuess.BgmFileTypeEnum.Wav:
                            Stream = AudioStreamWav.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                            break;
                        case BgmFileFormatGuess.BgmFileTypeEnum.Ogg:
                            Stream = AudioStreamOggVorbis.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                            break;
                        // 失败后使用默认的 OGG 格式进行读取
                        case BgmFileFormatGuess.BgmFileTypeEnum.Invalid:
                            bgmPath += ".ogg";
                            if (Godot.FileAccess.FileExists(bgmPath)) {
                                Stream = AudioStreamOggVorbis.LoadFromFile(bgmPath);
                            }
                            break;
                    }
                }
                // 如果没有外置覆盖 BGM 文件则使用内置 BGM
                Stream ??= entry.DefaultBgm;
                break;
            }
            
            if (play) Play();
        }
    }
    public void OnPlayerDied() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null");
        } else {
            if (!_levelConfig.FastRetry) {
                Stop();
            } else {
                _fadeOut = true;
            }
        }
    }
    public void OnPlayerStarmanStart() {
        Stop();
    }
    public void OnPlayerStarmanEnd() {
        Play();
    }
    public void OnTreeExiting() {
        // Fast Retry 记录 BGM 位置
        if (_levelConfig is not LevelConfig { FastRetry: true }) return;
        LevelManager.BgmPosition = GetPlaybackPosition();
        //GD.Print(LevelManager.BgmPosition);
    }
    public void Bgm146Sync() {
        if (_levelConfig is not { BgmId: 146 }) return;
        
        if (Playing && !_bgm146Player.Playing)
            _bgm146Player.Play(GetPlaybackPosition());
        if (!Playing && _bgm146Player.Playing)
            _bgm146Player.Playing = false;
    }
}
