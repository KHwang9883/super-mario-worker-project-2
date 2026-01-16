using Godot;
using System;
using System.IO;
using SMWP;
using SMWP.Level;
using SMWP.Level.Player;
using SMWP.Util;
using FileAccess = Godot.FileAccess;

public partial class BgmPlayer : AudioStreamPlayer {
    [Signal]
    public delegate void ModuleResourceConvertEventHandler(AudioStreamPlayer player, string filePath);
    
    [Export] private AudioStreamPlayer _bgm146Player = null!;
    private Node2D? _player;
    private PlayerDieAndHurt? _playerDieAndHurt;
    private PlayerSuit? _playerSuit;
    private LevelConfig? _levelConfig;
    private bool _fadeOut;

    private bool _isPlayerStarman;

    private float _onWaterVolume;
    private float _underWaterVolume;

    public enum LoadExternalTypeEnum { Override, Custom }

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        
        Callable.From(() => {
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            _playerDieAndHurt = (PlayerDieAndHurt)_player.GetMeta("PlayerDieAndHurt");
            _playerDieAndHurt.PlayerDiedSucceeded += OnPlayerDied;
            _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
            _playerSuit.PlayerStarmanStarted += OnPlayerStarmanStart;
            _playerSuit.PlayerStarmanFinishedBgmReset += OnPlayerStarmanEnd;
            
            // Checkpoint 场景控制元件更改 BGM 记录
            if (GameManager.ActivatedCheckpoints.Count > 0) {
                _levelConfig.BgmId = GameManager.CheckpointBgmId;
                //GD.Print($"_levelConfig.BgmId: {_levelConfig.BgmId}");
            }
            
            // Fast Retry 读取 BGM 位置
            if (_levelConfig.BgmId != GameManager.CurrentBgmId) {
                GameManager.BgmPosition = 0f;
            }
            
            SetBgm(false);
            Play(_levelConfig.FastRetry ? GameManager.BgmPosition : 0f);
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        // 过关与 Fast Retry 处理
        // Todo: 改为非FasterLevelPass时读取普通过关BgmId
        if (GameManager.IsLevelPass && Playing) {
            Stop();
            /*if (!_levelConfig.FasterLevelPass) {
                Play();
            }*/
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
            return;
        }

        switch (_levelConfig.BgmId) {
            // No Music
            case 600:
            case <= 0:  // SMWP v1.7 早期版本设置 No Music 的情况
                Stop();
                Stream = null;
                return;
            // 外置覆盖 BGM
            case < 627:
                LoadOverrideBgm();
                break;
            // BGM ID >= 627：自定义 BGM
            default:
                LoadCustomBgm();
                break;
        }

        // 设置的同时根据情况播放
        if (play && !_isPlayerStarman) Play();
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
    
    // 玩家无敌星状态 BGM 处理
    public void OnPlayerStarmanStart() {
        _isPlayerStarman = true;
        Stop();
    }
    public void OnPlayerStarmanEnd() {
        _isPlayerStarman = false;
        Play();
    }
    
    public void OnTreeExiting() {
        // Fast Retry 记录 BGM 位置
        if (_levelConfig is not LevelConfig { FastRetry: true }) return;
        GameManager.BgmPosition = GetPlaybackPosition();
        //GD.Print(GameManager.BgmPosition);
    }
    
    public void Bgm146Sync() {
        if (_levelConfig is not { BgmId: 146 }) return;
        
        if (Playing && !_bgm146Player.Playing)
            _bgm146Player.Play(GetPlaybackPosition());
        if (!Playing && _bgm146Player.Playing)
            _bgm146Player.Playing = false;
    }

    public void LoadOverrideBgm() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        foreach (var entry in _levelConfig.BgmDatabase.Entries) {
            if (entry.BgmId != _levelConfig.BgmId) continue;
            
            // 先清空原来的 BGM 资源
            Stream = null;
            
            //GD.Print($"Current LevelConfig BgmId: {_levelConfig.BgmId}");
                
            // 首先获取外置的覆盖 BGM
            foreach (var fileName in entry.FileNameForOverride) {
                // 遍历所有的兼容性文件名
                var baseDir = Path.GetDirectoryName(OS.GetExecutablePath());
                baseDir = baseDir?.Replace("\\", "/");
                var bgmPath =
                    baseDir + "/Data/" + entry.AlbumPath + "/" + fileName;
                // 因为之前 SMWP 版本后缀过于混乱，无法进行识别，故不保留文件后缀
                bgmPath = bgmPath.GetBaseName();
                //GD.Print($"bgmPath is: {bgmPath}");

                // 猜测 BGM 格式以及真实格式，并设置 Stream 资源
                LoadExternalBgmFile(bgmPath, LoadExternalTypeEnum.Override);
            }
            // 如果没有外置覆盖 BGM 文件则使用内置 BGM
            Stream ??= entry.DefaultBgm;
            break;
        }
    }
    public void LoadCustomBgm() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        // 读取ListConfig.ini
        var baseDir = Path.GetDirectoryName(OS.GetExecutablePath());
        baseDir = baseDir?.Replace("\\", "/");
        var customDir = 
            baseDir + "/Data/Custom/" + GameManager.CustomBgmPackage + "/";
        var listConfigPath = customDir + "ListConfig.ini";
        if (!FileAccess.FileExists(listConfigPath)) {
            GD.PushError($"{this}: Custom BGM ListConfig.ini not found at {listConfigPath}!");
            return;
        } 
            
        // 读取 ListConfig.ini 文件内容
        var file = FileAccess.Open(listConfigPath, FileAccess.ModeFlags.Read);
        if (file == null) {
            GD.PushError($"{this}: Failed to open ListConfig.ini at {listConfigPath}!");
            return;
        }
                    
        // 计算对应的行数（BgmId - 626）
        var lineIndex = _levelConfig.BgmId - 626;
        string? fileName = null;
                    
        // 按行读取，直到找到对应行
        for (var i = 1; i <= lineIndex; i++) {
            fileName = file.GetLine();
            // 如果文件行数不足，退出循环
            if (file.EofReached()) {
                break;
            }
        }
        file.Close();
                    
        // 检查是否成功读取到文件名
        if (string.IsNullOrEmpty(fileName)) {
            GD.PushError($"{this}: Custom BGM file name not found for BgmId {_levelConfig.BgmId}!");
            return;
        }
                    
        // 去除文件名的前后空格
        fileName = fileName.Trim();
        if (string.IsNullOrEmpty(fileName)) {
            GD.PushError($"{this}: Custom BGM file name is empty for BgmId {_levelConfig.BgmId}!");
            return;
        }
                    
        // 构建完整的文件路径（去除后缀）
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var bgmPath = customDir + fileNameWithoutExt;
                    
        // 猜测 BGM 格式以及真实格式，并设置 Stream 资源
        LoadExternalBgmFile(bgmPath, LoadExternalTypeEnum.Custom);
    }
    public void LoadExternalBgmFile(string bgmPath, LoadExternalTypeEnum loadType, string customDir = "", string fileName = "") {
        switch (BgmFileFormatGuess.GetGuessFormat(bgmPath)) {
            case BgmFileFormatGuess.BgmFileTypeEnum.Mp3:
                Stream = AudioStreamMP3.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                break;
            case BgmFileFormatGuess.BgmFileTypeEnum.Wav:
                Stream = AudioStreamWav.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                break;
            case BgmFileFormatGuess.BgmFileTypeEnum.Ogg:
                Stream = AudioStreamOggVorbis.LoadFromFile(
                    BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                break;

            // Module 格式需要额外转换
            // 由于 C# 无法直接调用 GDExtension 的类，因此发射信号交给 GDScript 处理
            case BgmFileFormatGuess.BgmFileTypeEnum.Mod:
            case BgmFileFormatGuess.BgmFileTypeEnum.It:
            case BgmFileFormatGuess.BgmFileTypeEnum.Xm:
                SetModuleBgm(bgmPath);
                break;

            // 失败后使用默认的 OGG 格式进行读取
            case BgmFileFormatGuess.BgmFileTypeEnum.Invalid:
                switch (loadType) {
                    // 外置的 override BGM
                    case LoadExternalTypeEnum.Override:
                        LoadOverrideBgmFileInvalid(bgmPath);
                        break;
                    // 外置的自定义 BGM
                    case LoadExternalTypeEnum.Custom:
                        LoadCustomBgmFileInvalid(bgmPath, customDir, fileName);
                        break;
                }
                break;
        }
    }

    public void SetModuleBgm(string bgmPath) {
        //Callable.From(() => {
        EmitSignal(
            SignalName.ModuleResourceConvert, this,
            BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
        );
        //}).CallDeferred();
    }
    
    public void LoadOverrideBgmFileInvalid(string bgmPath) {
        bgmPath += ".ogg";
        if (FileAccess.FileExists(bgmPath)) {
            Stream = AudioStreamOggVorbis.LoadFromFile(bgmPath);
        }
    }

    public void LoadCustomBgmFileInvalid(string bgmPath, string customDir, string fileName) {
        var fullPath = customDir + fileName;
        if (FileAccess.FileExists(fullPath)) {
            // 尝试直接加载带后缀的文件
            try {
                // 根据文件后缀直接加载
                var ext = Path.GetExtension(fileName).ToLower();
                switch (ext) {
                    case ".mp3":
                        Stream = AudioStreamMP3.LoadFromFile(fullPath);
                        break;
                    case ".wav":
                        Stream = AudioStreamWav.LoadFromFile(fullPath);
                        break;
                    case ".ogg":
                        Stream = AudioStreamOggVorbis.LoadFromFile(fullPath);
                        break;
                    case ".mod":
                    case ".it":
                    case ".xm":
                        EmitSignal(SignalName.ModuleResourceConvert, this, fullPath);
                        break;
                    default:
                        GD.PushError($"{this}: Unsupported file format for custom BGM: {ext}!");
                        break;
                }
            } catch (Exception e) {
                GD.PushError($"{this}: Failed to load custom BGM file: {fullPath}, Error: {e.Message}!");
            }
        } else {
            GD.PushError($"{this}: Custom BGM file not found: {fullPath}!");
        }
    }
    
    // BgmId 不合法自动为 No Music
}
