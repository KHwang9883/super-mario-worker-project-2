using Godot;
using System;

public partial class SuperLeafMovement : Node {
    [Export] private Area2D _leaf = null!;
    [Export] private Sprite2D _sprite2D = null!;
    [Export] private float _risingSpeedY = -7f;
    [Export] private float _gravity = 0.2f;

    [Export] private float _floatingSpeedY = 6f;
    [Export] private float _floatingDistanceHorizontal = 32f;
    [Export] private float _floatingSpeedYRange = 2.2f;

    public enum SuperLeafMoveStatus { Rising, Falling }
    private SuperLeafMoveStatus _status = SuperLeafMoveStatus.Rising;
    private float _speedY = -7f;
    private float _originPosX;
    private float _angle;

    public override void _Ready() {
        _speedY = _risingSpeedY;
    }
    public override void _PhysicsProcess(double delta) {
        switch (_status) {
            case SuperLeafMoveStatus.Rising:
                _speedY += _gravity;
                _leaf.Position += new Vector2(0, _speedY);
                if (_speedY >= 0f) {
                    _originPosX = _leaf.Position.X;
                    _status = SuperLeafMoveStatus.Falling;
                }
                break;
            case SuperLeafMoveStatus.Falling:
                var trueAngle = Mathf.DegToRad(_angle);
                _speedY = (Mathf.Sin(
                    Mathf.DegToRad((_angle % 180) * 0.5f + 180f)
                    ) + 1f) * _floatingSpeedYRange;
                _leaf.Position = new Vector2(
                    _originPosX - (Mathf.Cos(trueAngle) - 1f) * _floatingDistanceHorizontal,
                    _leaf.Position.Y + _speedY
                    );
                _angle += _floatingSpeedY;
                _sprite2D.FlipH = _angle % 360 > 180;
                break;
        }
    }
}
