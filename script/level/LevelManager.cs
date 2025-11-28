using Godot;
using System;
using Godot.Collections;
using SMWP.Level.Sound;
using Array = Godot.Collections.Array;
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace SMWP.Level;

public partial class LevelManager : Node {
    [Signal]
    public delegate void PlaySound1UPEventHandler();
    
    public static int Time { get; set; }
    public static int Life { get; set; } = 3;
    public static int Score { get; set; }
    public static int Coin { get; set; } = 99;

    public static int CurrentCheckpointId;
    public static Array<int> ActivatedCheckpoints = []; 

    public static bool IsGameOver;
    public static bool IsLevelPass;

    // for test only
    public static bool IsGodMode = true;
    
    [Export] private ContinuousAudioStream2D _1UPAudioStream2DNode = null!;
    public static ContinuousAudioStream2D Sound1UPAudioStream2D = null!;
    private static Array<Node> _timeClearSounds = null!;
    
    // Todo: 关卡标题
    public static string LevelTitle = "STARLAND\nLEVEL";

    private static int _levelTimeTimer;
    private static int _levelPassTimer;
    private static int _timeClearTimer;
    private static int _timeClearedTimer;
    public static float BgmPosition;
    public static Node2D? Player;

    public override void _Ready() {
        Sound1UPAudioStream2D = _1UPAudioStream2DNode;
        _timeClearSounds = GetNode("TimeClearSoundGroup").GetChildren();
    }
    
    public override void _PhysicsProcess(double delta) {
        // 不在关卡中
        if (Player == null) return;
        
        // 关卡计时
        TimeCount();
        
        // 过关
        LevelPass();
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
    }
    
    // Level Timer
    public static void SetLevelTime(int setTime) {
        Time = setTime;
        _levelTimeTimer = 0;
    }
    public void TimeCount() {
        if (IsLevelPass) return;
        // 传送时，计时器停止
        // Todo: if (playerMovement.Stuck or PipeIn/Out) return;
        // 玩家死亡，计时器停止
        if (Player != null && Player.ProcessMode == ProcessModeEnum.Disabled) return;
        
        if (Time <= 0) return;
        _levelTimeTimer++;
        if (_levelTimeTimer < 16) return;
        _levelTimeTimer = 0;
        Time--;
    }
    
    // Level Pass
    public void LevelPass() {
        if (!IsLevelPass) return;
        
        _levelPassTimer++;
        if (_levelPassTimer <= 450) return;
        // 时间结算
        if (_levelPassTimer > 450 && Time> 0) {
            Time -= 1; AddScore(100);
            _timeClearTimer += 1;
        }
        if (_levelPassTimer > 500 && Time> 9) {
            Time -= 10; AddScore(1000);
            _timeClearTimer += 1;
        }
        if (_levelPassTimer > 650 && Time> 99) {
            Time -= 100; AddScore(10000);
            _timeClearTimer += 1;
        }
        if (_timeClearTimer > 5) {
            _timeClearTimer = 0;
            PlaySoundClearTime();
        }
        if (Time == 0) {
            _timeClearedTimer++;
            if (_timeClearTimer >= 50) {
                _timeClearedTimer = 0;
                
                _timeClearTimer = 0;
                _levelPassTimer = 0;
                JumpToLevel();
            }
        }
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
        
        // Todo: 关卡跳转 / 回到标题画面 / 编辑界面
        
        GetTree().Free();
    }
}
