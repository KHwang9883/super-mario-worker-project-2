using Godot;
using System;
using System.IO;
using SMWP.Level;
using SMWP.Level.Player;
using SMWP.Level.Tool;
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
            GD.PushError($"{this}: LevelConfig is null!");
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
            if (_levelConfig.BgmId < 627) {
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
                                Stream = AudioStreamOggVorbis.LoadFromFile(
                                    BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                                break;

                            // Module 格式需要额外转换
                            // 由于 C# 无法直接调用 GDExtension 的类，因此发射信号交给 GDScript 处理
                            case BgmFileFormatGuess.BgmFileTypeEnum.Mod:
                                EmitSignal(
                                    SignalName.ModuleResourceConvert, this,
                                    BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                                );
                                break;
                            case BgmFileFormatGuess.BgmFileTypeEnum.It:
                                EmitSignal(
                                    SignalName.ModuleResourceConvert, this,
                                    BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                                );
                                break;
                            case BgmFileFormatGuess.BgmFileTypeEnum.Xm:
                                EmitSignal(
                                    SignalName.ModuleResourceConvert, this,
                                    BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                                );
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
            } else {
                // BGM ID >= 627：自定义 BGM
                // 读取ListConfig.ini
                var baseDir = Path.GetDirectoryName(OS.GetExecutablePath());
                baseDir = baseDir?.Replace("\\", "/");
                var customDir = 
                    baseDir + "/Data/Custom/" + LevelManager.CustomBgmPackage + "/";
                var listConfigPath = customDir + "ListConfig.ini";
                if (!Godot.FileAccess.FileExists(listConfigPath)) {
                    GD.PushError($"{this}: Custom BGM ListConfig.ini not found at {listConfigPath}!");
                } else {
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
                    
                    // 猜测 BGM 格式以及真实格式
                    switch (BgmFileFormatGuess.GetGuessFormat(bgmPath)) {
                        case BgmFileFormatGuess.BgmFileTypeEnum.Mp3:
                            Stream = AudioStreamMP3.LoadFromFile(BgmFileFormatGuess.GetFullBgmFileName(bgmPath));
                            // Todo: 待测试
                            GD.Print(Stream);
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
                            EmitSignal(
                                SignalName.ModuleResourceConvert, this,
                                BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                            );
                            break;
                        case BgmFileFormatGuess.BgmFileTypeEnum.It:
                            EmitSignal(
                                SignalName.ModuleResourceConvert, this,
                                BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                            );
                            break;
                        case BgmFileFormatGuess.BgmFileTypeEnum.Xm:
                            EmitSignal(
                                SignalName.ModuleResourceConvert, this,
                                BgmFileFormatGuess.GetFullBgmFileName(bgmPath)
                            );
                            break;
                            
                        // 失败后尝试使用原始文件名（带后缀）
                        case BgmFileFormatGuess.BgmFileTypeEnum.Invalid:
                            var fullPath = customDir + fileName;
                            if (Godot.FileAccess.FileExists(fullPath)) {
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
                            break;
                    }
                }
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
