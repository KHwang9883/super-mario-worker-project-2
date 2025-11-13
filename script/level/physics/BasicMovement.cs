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

    //private bool _debugBool;
    
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
            //GD.Print("==========");
            //GD.Print(MoveObject.Position);
            var originPosition = MoveObject.Position;
            MoveObject.Position =
                new Vector2(originPosition.X + 33f * Mathf.Sign(SpeedX), originPosition.Y + 20f);
            MoveObject.ForceUpdateTransform();
            // MoveAndCollide的safeMargin参数必须为一个较小值，否则运动体会有约半截卡进地面边缘，原因未知
            var collision = MoveObject.MoveAndCollide(Vector2.Zero, true, 0.01f);
            //GD.Print(collision);
            if (collision == null) {
                SpeedX *= -1f;
                //GD.Print(MoveObject.Position);
                //_debugBool = true;
                //MoveObject.ProcessMode = ProcessModeEnum.Disabled;
            }
            //if (_debugBool) return;
            MoveObject.Position = originPosition;
            MoveObject.ForceUpdateTransform();
            //GD.Print(MoveObject.Position);
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

        SetJumpSpeed();
    }
    public void OnScreenEntered() {
        SetMovementDirection();
    }
    public virtual void SetMovementDirection() {
        if (!InitiallyFaceToPlayer) return;
        if (MoveObject.Position.X < Player?.Position.X) {
            SpeedX = Mathf.Abs(SpeedX);
        } else if (MoveObject.Position.X > Player?.Position.X) {
            SpeedX = -Mathf.Abs(SpeedX);
        }
    }
    public virtual void SetJumpSpeed() {
        if (MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Min(0f, JumpSpeed);
        }
    }
}
