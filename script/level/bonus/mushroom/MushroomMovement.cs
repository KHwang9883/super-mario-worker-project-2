using Godot;
using System;

public partial class MushroomMovement : BasicMovement {
    public bool Turning;
    
    public override void _PhysicsProcess(double delta) {
        // x 速度
        if (MoveObject.IsOnWall()) {
            Turning = true;
            SpeedX *= -1f;
        }
        
        // y 速度
        if (!MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed);
        } else {
            SpeedY = 0f;
        }
        
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();
    }
    public void OnTurned() {
        Turning = false;
    }
}
