using Godot;
using System;

public partial class PlatformHorizontal : AnimatableBody2D {
    [Export] public float SpeedX = -1f;

    public override void _PhysicsProcess(double delta) {
        // 卡路里，统一向右运动
        if (MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
            Position += new Vector2(Mathf.Abs(SpeedX), 0f);
            return;
        }
        
        // General Movement
        if (MoveAndCollide(new Vector2(SpeedX, 0f), false, 0.02f) != null) {
            SpeedX = -SpeedX;
        }
    }
}
