using Godot;
using System;
using System.Runtime.InteropServices.Swift;

public partial class BasicMovement : Node {
    [Export] public CharacterBody2D MoveObject = null!;
    [Export] public float SpeedX;
    [Export] public float Gravity = 0.5f;
    [Export] public float MaxFallSpeed = 999f;
    [Export] public float JumpSpeed;
    [Export] public bool EdgeDetect;
    protected const float FramerateOrigin = 50f;
    protected float SpeedY;
    
    public override void _Ready() {
        MoveObject = (CharacterBody2D)GetParent();
    }
    
    public override void _PhysicsProcess(double delta) {
        // x 速度
        if (MoveObject.IsOnWall()) {
            SpeedX *= -1f;
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
