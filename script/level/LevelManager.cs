using Godot;
using System;
using SMWP.Level.Sound;

namespace SMWP.Level;

public partial class LevelManager : Node {
    [Signal]
    public delegate void PlaySound1UPEventHandler();
    public static int Time { get; set; }
    public static int Life { get; set; } = 1;
    public static int Score { get; set; }
    public static int Coin { get; set; }

    public static bool IsGameOver;
    public static bool IsLevelPass;
    
    [Export] private ContinuousAudioStream2D _1UPAudioStream2DNode = null!;
    public static ContinuousAudioStream2D Sound1UPAudioStream2D = null!;
    
    // Todo: 关卡标题
    public static string? LevelTitle;

    private int _levelTimeTimer;
    private int _levelPassTimer;
    private Node? _player;

    public override void _Ready() {
        Sound1UPAudioStream2D = _1UPAudioStream2DNode;
        _player ??= GetTree().GetFirstNodeInGroup("player");
    }
    
    public override void _PhysicsProcess(double delta) {
        // 不在关卡中
        if (_player == null) return;
        
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
    public void TimeCount() {
        if (IsLevelPass) return;
        // 传送时，计时器停止
        // Todo: if (playerMovement.Stuck or PipeIn/Out) return;
        // 玩家死亡，计时器停止
        if (_player != null && _player.ProcessMode == ProcessModeEnum.Disabled) return;
        
        _levelTimeTimer++;
        if (_levelTimeTimer < 16) return;
        if (Time <= 0) return;
        _levelTimeTimer = 0;
        Time--;
    }
    
    // Level Pass
    public void LevelPass() {
        if (!IsLevelPass) return;
        _levelPassTimer++;
        if (_levelPassTimer < 1000) return;
        // Todo: 时间结算
    }
}
