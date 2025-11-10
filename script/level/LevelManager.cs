using Godot;
using System;

namespace SMWP.Level;

public partial class LevelManager : Node {
    public static int Time { get; set; }
    public static int Life { get; set; } = 4;
    public static int Score { get; set; }
    public static int Coin { get; set; }

    // Todo: 关卡标题
    public static string? LevelTitle;

    private int _timer;

    // Level Timer
    public override void _PhysicsProcess(double delta) {
        
        // 传送时，计时器停止
        // Todo: if (playerMovement.Stuck or PipeIn/Out) return;
        // 玩家死亡，计时器停止
        if (GetTree().GetFirstNodeInGroup("player").ProcessMode == ProcessModeEnum.Disabled) return;
        
        _timer++;
        if (_timer < 16) return;
        if (Time <= 0) return;
        _timer = 0;
        Time--;
    }
    public static void AddScore(int score) {
        // 1UP 占用 -1 分
        if (score == -1) {
            AddLife();
        }
        // AddScore方法不支持减分
        Score += (score > 0) ? score : 0;
    }
    public static void AddLife() {
        Life++;
    }
}
