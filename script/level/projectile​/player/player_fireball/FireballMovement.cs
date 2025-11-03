using Godot;
using System;

public partial class FireballMovement : BasicMovement {
    [Signal]
    public delegate void FireballExplodeEventHandler();
    public float Direction { get; set; } = 1f;
    
    public override void _Ready() {
        SpeedX = Mathf.Abs(SpeedX) * Direction;
    }

    public override void _PhysicsProcess(double delta) {
        // x 速度
        if (MoveObject.IsOnWall()) {
            SpeedX *= -1f;
        }
        
        if (MoveObject.IsOnWall()) {
            EmitSignal(SignalName.FireballExplode);
            MoveObject.QueueFree();
        }
        
        // y 速度
        if (!MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed);
        } else {
            SpeedY = Mathf.Min(0f, JumpSpeed);
        }
        
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();
    }
}
