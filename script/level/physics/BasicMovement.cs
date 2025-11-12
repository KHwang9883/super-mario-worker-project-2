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
    public CharacterBody2D? Player;
    
    public override void _Ready() {
        MoveObject = (CharacterBody2D)GetParent();
        Player = (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        if (InitiallyFaceToPlayer) {
            SetMovementDirection();
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        // 自动转向检测
        if (EdgeDetect && MoveObject.IsOnFloor()) {
            var originPosition = MoveObject.GetGlobalPosition();
            MoveObject.SetGlobalPosition(
                new Vector2(originPosition.X + 30f * Mathf.Sign(SpeedX), originPosition.Y + 20f)
                );
            var collision = MoveObject.MoveAndCollide(Vector2.Zero, true);
            if (collision == null) {
                SpeedX *= -1f;
            }
            MoveObject.SetGlobalPosition(originPosition);
        }
        
        // x 速度
        if (MoveObject.IsOnWall()) {
            SpeedX *= -1f;
        }
        
        // y 速度
        if (!MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed);
        }
        
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();

        if (MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Min(0f, JumpSpeed);
        }
    }
    public void OnScreenEntered() {
        SetMovementDirection();
    }
    public virtual void SetMovementDirection() {
        if (!InitiallyFaceToPlayer) return;
        if (MoveObject.GlobalPosition.X < Player?.GlobalPosition.X) {
            SpeedX = Mathf.Abs(SpeedX);
        } else if (MoveObject.GlobalPosition.X > Player?.GlobalPosition.X) {
            SpeedX = -Mathf.Abs(SpeedX);
        }
    }
}
