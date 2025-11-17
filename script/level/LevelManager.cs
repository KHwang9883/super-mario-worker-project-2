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
    
    [Export] private ContinuousAudioStream2D _1UPAudioStream2DNode = null!;
    public static ContinuousAudioStream2D Sound1UPAudioStream2D = null!;
    
    // Todo: 关卡标题
    public static string? LevelTitle;

    private int _timer;

    public override void _Ready() {
        Sound1UPAudioStream2D = _1UPAudioStream2DNode;
    }
    
    // Level Timer
    public override void _PhysicsProcess(double delta) {
        var player = GetTree().GetFirstNodeInGroup("player");
        
        // 不在关卡中
        if (player == null) return;
        
        // 传送时，计时器停止
        // Todo: if (playerMovement.Stuck or PipeIn/Out) return;
        // 玩家死亡，计时器停止
        if (player.ProcessMode == ProcessModeEnum.Disabled) return;
        
        _timer++;
        if (_timer < 16) return;
        if (Time <= 0) return;
        _timer = 0;
        Time--;
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
}
