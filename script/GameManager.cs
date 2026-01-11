using Godot;
using System;
using System.IO;
using Godot.Collections;
using SMWP.Level.Data;
using SMWP.Level.Player;
using SMWP.Level.Sound;
using Array = Godot.Collections.Array;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace SMWP;

public partial class GameManager : Node {
    [Signal]
    public delegate void PlaySound1UPEventHandler();
    [Signal]
    public delegate void ScenarioNextLevelEventHandler();

    public static bool TitleScreenAnimationFinished;
    
    public static bool GamePause;

    public static SmwlLevelData? LevelFileData;
    public static string? ScenarioFile;
    
    public static int Time { get; set; }
    public static int Life { get; set; } = 4;
    public static int Score { get; set; }
    public static int Coin { get; set; }

    public static int InitialLivesOfSingleLevel = 4;

    // 浣熊装测试
    /*
    public static PlayerSuit.SuitEnum PlayerSuitRestore = PlayerSuit.SuitEnum.Powered;
    public static PlayerSuit.PowerupEnum PlayerPowerupRestore = PlayerSuit.PowerupEnum.Raccoon;
    */
    
    public static PlayerSuit.SuitEnum PlayerSuitRestore = PlayerSuit.SuitEnum.Small;
    public static PlayerSuit.PowerupEnum PlayerPowerupRestore = PlayerSuit.PowerupEnum.Fireball;
    
    public static int CurrentCheckpointId;
    public static Array<int> ActivatedCheckpoints = [];

    public static bool IsCheckpointWaterHeightRecorded;
    public static float CheckpointWaterHeight;
    
    public static int CheckpointBgpId = 1;
    public static int CheckpointRainyLevel;
    public static int CheckpointFallingStarsLevel;
    public static int CheckpointSnowyLevel;
    public static int CheckpointThunderLevel;
    public static int CheckpointWindyLevel;
    public static int CheckpointDarkness;
    public static int CheckpointBrightness;

    public static bool IsGameOver;
    public static bool IsLevelPass;
    
    [Export] private ContinuousAudioStream2D _1UPAudioStream2DNode = null!;
    public static ContinuousAudioStream2D Sound1UPAudioStream2D = null!;
    private static Array<Node> _timeClearSounds = null!;

    public static bool TimeCountPause;
    private static int _levelTimeTimer;
    private static int _levelPassTimer;
    public static bool IsFasterLevelPass;
    private static int _timeClearTimer;
    private static int _timeClearedTimer;

    public static bool IsPlayingScenario;
    public static int ScenarioLevelCount;
    public static int CurrentScenarioLevel;
    public static Dictionary<int, int> ScenarioNewLevelLineNum = [];
    
    public static int CurrentBgmId;
    public static float BgmPosition;
    public static string CustomBgmPackage = "Example";
    
    public static bool IsColorAccessibilityMode;
    
    // Todo: Edit 部分完成前在全局设置界面中可调
    public static bool IsGodMode;

    public static bool ShowFps;
    
    // Todo: Edit Options (TemporaryFiles)
    public static int TemporaryFiles = 100;
    
    public static Node2D? Player;
    public static PlayerMovement? PlayerMovementNode;

    public override void _Ready() {
        Sound1UPAudioStream2D = _1UPAudioStream2DNode;
        _timeClearSounds = GetNode("TimeClearSoundGroup").GetChildren();
        
        // 读取配置文件
        ConfigManager.LoadConfig();
    }
    
    public override void _PhysicsProcess(double delta) {
        // 不在关卡中
        if (Player == null) return;
        
        // 关卡计时
        TimeCount();
        
        // 过关
        LevelPass();
    }

    public static void SetCheckpointWaterHeight(float height) {
        CheckpointWaterHeight = height;
        IsCheckpointWaterHeightRecorded = true;
    }
    // 加分
    public static void AddScore(int score) {
        // 1UP 占用 -1 分
        if (score == -1) {
            AddLife();
        }
        // AddScore方法不支持减分
        Score += (score > 0) ? score : 0;
    }
    
    // 奖命
    public static void AddLife() {
        Life++;
        Sound1UPAudioStream2D.Play();
    }
    
    // Game Over Clear
    public static void GameOverClear() {
        Life = 4;
        Coin = 0;
        Score = 0;

        PlayerSuitRestore = PlayerSuit.SuitEnum.Small;
        
        IsPlayingScenario = false;
        ScenarioFile = null;
        ScenarioLevelCount = 0;
        CurrentScenarioLevel = 0;
        ScenarioNewLevelLineNum = [];
    }
    
    // Level Timer
    public static void SetLevelTime(int setTime) {
        Time = setTime;
        _levelTimeTimer = 0;
    }
    public void TimeCount() {
        if (TimeCountPause || IsLevelPass || GamePause) return;
        
        if (!IsInstanceValid(Player)) return;
        
        // 传送时，计时器停止
        PlayerMovementNode = (PlayerMovement)Player.GetMeta("PlayerMovement");
        if (PlayerMovementNode.IsInPipeTransport) return;
        
        // 玩家死亡，计时器停止
        if (Player is { ProcessMode: ProcessModeEnum.Disabled }) return;
        
        if (Time <= 0) return;
        _levelTimeTimer++;
        if (_levelTimeTimer < 16) return;
        _levelTimeTimer = 0;
        Time--;
    }
    
    // Level Pass
    public void LevelPass() {
        if (!IsLevelPass) return;

        Player!.ProcessMode = ProcessModeEnum.Disabled;
        
        _levelPassTimer++;
        
        int baseTimeThreshold = IsFasterLevelPass ? 50 : 450;
        int tenTimeThreshold = IsFasterLevelPass ? 100 : 500;
        int hundredTimeThreshold = IsFasterLevelPass ? 250 : 650;
        
        if (_levelPassTimer <= baseTimeThreshold) return;
        
        // 时间结算
        if (_levelPassTimer > baseTimeThreshold && Time > 0) {
            Time -= 1; 
            AddScore(100);
            _timeClearTimer += 1;
        }
        if (_levelPassTimer > tenTimeThreshold && Time > 9) {
            Time -= 10; 
            AddScore(1000);
            _timeClearTimer += 1;
        }
        if (_levelPassTimer > hundredTimeThreshold && Time > 99) {
            Time -= 100; 
            AddScore(10000);
            _timeClearTimer += 1;
        }
        if (_timeClearTimer > 5) {
            _timeClearTimer = 0;
            PlaySoundClearTime();
        }
        
        if (Time == 0) {
            _timeClearedTimer++;
            if (_timeClearedTimer >= 50) {
                _timeClearedTimer = 0;
                _timeClearTimer = 0;
                _levelPassTimer = 0;
                JumpToLevel();
            }
        }
        
        // 不限时关卡小幅等待后直接跳转
        if (Time < 0) {
            _timeClearTimer = 0;
            _levelPassTimer = 0;
            JumpToLevel();
        }
    }
    public void PlaySoundClearTime() {
        foreach (var node in _timeClearSounds) {
            if (node is not ContinuousAudioStream2D sound) continue;
            if (sound.Playing) continue;
            sound.Play();
            // 挑选随机一个节点播放
            break;
        }
    }

    public static void AddCoin(int coin) {
        Coin += coin;
        if (Coin >= 100) {
            Coin -= 100;
            AddLife();
        }
    }
    public void JumpToLevel() {
        // 解除游戏暂停状态
        GetTree().Paused = false;

        // 清空单关范围的全局变量
        IsLevelPass = false;
        Player = null;
        PlayerMovementNode = null;

        TimeCountPause = false;
        GamePause = false;
        
        CurrentCheckpointId = 0;
        ActivatedCheckpoints = [];
        
        IsCheckpointWaterHeightRecorded = false;
        CheckpointWaterHeight = 0;
        CurrentBgmId = 0;
        CheckpointBgpId = 0;
        CheckpointRainyLevel = 0;
        CheckpointFallingStarsLevel = 0;
        CheckpointSnowyLevel = 0;
        CheckpointThunderLevel = 0;
        CheckpointWindyLevel = 0;
        CheckpointDarkness = 0;
        CheckpointBrightness = 0;

        IsGameOver = false;
        IsLevelPass = false;
        
        // Todo: 关卡跳转 / 回到标题画面 / 编辑界面
        
        if (!IsPlayingScenario) {
            // 清空关卡文件数据
            LevelFileData = null;
            // 非 Scenario 下一关则 PlayerSuitRestore 等状态清空
            GameOverClear();
            GetTree().ChangeSceneToFile("uid://2h2s1iqemydd");
        }
        
        else {
            CurrentScenarioLevel++;
            if (CurrentScenarioLevel >= ScenarioLevelCount) {
                GameOverClear();
                ScenarioFile = null;
                GetTree().ChangeSceneToFile("uid://2h2s1iqemydd");
            } else {
                // 下一个关卡：删除当前关卡
                EmitSignal(SignalName.ScenarioNextLevel);
            }
        }
    }
}
