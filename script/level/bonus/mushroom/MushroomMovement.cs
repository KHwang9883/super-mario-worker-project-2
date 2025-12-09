using Godot;
using System;
using SMWP.Level.Physics;

namespace SMWP.Level.Bonus.Mushroom;

public partial class MushroomMovement : BasicMovement {
    [Export] private BonusSprout _bonusSprout = null!;
    
    [Export] private CollisionShape2D _collisionShape2D = null!;
    [Export] private MushroomAnimation _mushroomAnimation = null!;
    
    public bool Turning;
    private bool _turned;
    private float _originalSpeedX;
    
    public override void _PhysicsProcess(double delta) {
        // 不在 Sprout 状态才开始执行
        if (_bonusSprout.Overlapping) return;

        // x 速度
        if (MoveObject.IsOnWall() && !_turned) {
            Turning = true;
            _originalSpeedX = SpeedX;
        }

        if (_turned) {
            _turned = false;
        }
        
        // 转向中
        // 应用 Scale 和 Position 变化
        var shape = _collisionShape2D.Shape;
        
        _collisionShape2D.Scale = new Vector2(1f - _mushroomAnimation.AnimationFrameScaleX / 20f, 1f/* - _mushroomAnimation.AnimationFrameScaleY / 20f*/);
        _collisionShape2D.GlobalPosition = new Vector2(
            MoveObject.GlobalPosition.X + _mushroomAnimation.AnimationFrameScaleX * 1.0f * Mathf.Sign(SpeedX),
            _collisionShape2D.GlobalPosition.Y);
        
        // y 速度
        if (!MoveObject.IsOnFloor()) {
            SpeedY = Mathf.Clamp(SpeedY + Gravity, -999f, MaxFallSpeed);
        }
        
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();

        if (MoveObject.IsOnFloor()) {
            SpeedY = 0f;
        }
    }
    public void OnTurned() {
        Turning = false;
        _turned = true;
        SpeedX = _originalSpeedX * -1f;
    }
}
