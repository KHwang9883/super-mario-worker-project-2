using Godot;
using System;
using System.Runtime.InteropServices.Swift;

namespace SMWP.Level.Physics;

[GlobalClass]
public partial class BasicMovement : Node {
    [Export] public CharacterBody2D MoveObject = null!;
    [Export] public bool InitiallyFaceToPlayer = true;
    [Export] public float SpeedX;
    [Export] public float SpeedY;
    [Export] public float Gravity = 0.5f;
    [Export] public float MaxFallSpeed = 999f;
    [Export] public float JumpSpeed;
    [Export] public bool EdgeDetect;
    protected const float FramerateOrigin = 50f;
    private CharacterBody2D? _player;
    
    public override void _Ready() {
        MoveObject = (CharacterBody2D)GetParent();
        _player = (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        if (InitiallyFaceToPlayer) {
            SetMovementDirection();
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        // x 速度
        if (MoveObject.IsOnWall()) {
            SpeedX *= -1f;
        }
        
        // y 速度
        SpeedY = !MoveObject.IsOnFloor() ?
            Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed) : Mathf.Min(0f, JumpSpeed);
        
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();
    }
    public void OnScreenEntered() {
        SetMovementDirection();
    }
    public void SetMovementDirection() {
        if (!InitiallyFaceToPlayer) return;
        if (MoveObject.GlobalPosition.X < _player?.GlobalPosition.X) {
            SpeedX = Mathf.Abs(SpeedX);
        } else if (MoveObject.GlobalPosition.X > _player?.GlobalPosition.X) {
            SpeedX = -Mathf.Abs(SpeedX);
        }
    }
}
