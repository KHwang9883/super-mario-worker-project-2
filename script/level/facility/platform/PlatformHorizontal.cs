using Godot;
using System;

public partial class PlatformHorizontal : AnimatableBody2D {
    [Export] public float SpeedX = -1f;

    public override void _PhysicsProcess(double delta) {
        // 卡路里，统一向右运动
        if (MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
            Position += new Vector2(Mathf.Abs(SpeedX), 0f);
            return;
        } else {
            // General Movement
            
            // 这样写是为了防止直接使用 MoveAndCollide 带 SafeMargin 导致的平台上边界碰到天花板等问题
            var originPos = Position;
            Position += new Vector2(SpeedX, 0f);
            
            // 原地检测
            if (MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
                SpeedX = -SpeedX;
                Position = originPos;
                MoveAndCollide(new Vector2(SpeedX, 0f), false, 0.02f);
            }
        }
    }
}
