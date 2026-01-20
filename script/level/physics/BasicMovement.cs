using Godot;
using System;
using System.Runtime.InteropServices.Swift;

namespace SMWP.Level.Physics;

[GlobalClass]
public partial class BasicMovement : Node {
    /*[Signal]
    public delegate void MoveProcessDelegateEventHandler(Vector2 velocity);*/
        
    [Export] public CharacterBody2D MoveObject = null!;
    [Export] public bool InitiallyFaceToPlayer = true;
    [Export] public float SpeedX;
    [Export] public float SpeedY;
    [Export] public float Gravity = 0.5f;
    [Export] public float MaxFallSpeed = 999f;
    [Export] public float JumpSpeed;
    [Export] public bool EdgeDetect;
    
    protected const float FramerateOrigin = 50f;
    protected CharacterBody2D? Player;

    private bool _notInWall;
    
    public override void _Ready() {
        MoveObject = (CharacterBody2D)GetParent();
        Player = (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        if (InitiallyFaceToPlayer) {
            SetMovementDirection();
        }
    }
    public override void _PhysicsProcess(double delta) {
        TurnDetect();
        
        SpeedXProcess();
        
        SpeedYProcess();
        
        ApplySpeed();

        Move();

        SetJumpSpeed();
    }
    public void OnScreenEntered() {
        SetMovementDirection();
    }
    public void TurnDetect() {
        // 自动转向检测
        if (EdgeDetect && MoveObject.IsOnFloor()) {
            var originPosition = MoveObject.Position;
            MoveObject.Position +=
                // y 方向上的平移检测交由 MoveAndCollide 处理，否则可能有遇到悬崖或墙角提前转向的问题
                new Vector2(33f * Mathf.Sign(SpeedX), 0f /*20f*/);
            MoveObject.ForceUpdateTransform();
            // MoveAndCollide 的 safeMargin 参数必须为一个较小值，否则运动体会有约半截卡进地面边缘，原因未知
            var collision = MoveObject.MoveAndCollide(Vector2.Down * 20f, true, 0.05f);
            if (collision == null) {
                SpeedX *= -1f;
            }
            MoveObject.Position = originPosition;
            MoveObject.ForceUpdateTransform();
        }
    }
    public virtual void SpeedXProcess() {
        // x 速度
        if (MoveObject.IsOnWall()) {
            SpeedX *= -1f;
        }
    }
    public virtual void SpeedYProcess() {
        // y 速度
        // 重力微调
        
        if (SpeedY == 0f) {
            SpeedY += Gravity * 4;
        }
        
        if (!MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed);
        }
    }
    public void ApplySpeed() {
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
    }
    public virtual void Move() {
        // 针对大部分敌人运动：卡墙处理
        if (MoveObject.MoveAndCollide(Vector2.Zero, true, 1f) == null) {
            var originPosition = MoveObject.Position;
            MoveObject.MoveAndSlide();
            if (!_notInWall) {
                var obj = MoveObject;
                var isInWall = obj.IsOnWall() || obj.IsOnFloor() || obj.IsOnCeiling();
                if (isInWall) {
                    MoveObject.Position = originPosition;
                } else {
                    _notInWall = true;
                }
            }
        }
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
