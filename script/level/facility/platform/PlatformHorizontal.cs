using Godot;
using System;

public partial class PlatformHorizontal : AnimatableBody2D {
    [Export] public float SpeedX = 1f;
    [Export] private Area2D? _overlapArea2D;
    private bool _turning;
    private float _targetSpeedX;

    public override void _PhysicsProcess(double delta) {
        // 转向状态下无视碰撞检测，直到达到目标速度
        if (!_turning) {
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
        } else {
            Position += new Vector2(SpeedX, 0f);
        }
        
        // 转向检测
        if (_overlapArea2D == null) return;
        
        _overlapArea2D.GlobalPosition = GlobalPosition + new Vector2(SpeedX, 0f);
        
        if (!_turning) {
            var bodies = _overlapArea2D.GetOverlappingAreas();
            if (bodies.Count == 0) return;
            _targetSpeedX = -SpeedX;
            _turning = true;
        }
        
        // 转向成功后立即进入转向过程
        if (!_turning) return;
        SpeedX = Mathf.MoveToward(SpeedX, _targetSpeedX, 0.1f);
        if (Math.Abs(SpeedX - _targetSpeedX) < 0.02f) {
            SpeedX = _targetSpeedX;
            _turning = false;
        }
        
        // Todo: 开关砖第二功能：SpeedX 和 _targetSpeedX 均 *= -1
    }
}
