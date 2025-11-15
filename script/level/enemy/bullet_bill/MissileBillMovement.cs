using Godot;
using System;
using SMWP.Level.Physics;

public partial class MissileBillMovement : BasicMovement {
    [Export] private float _maxSpeedX = 3.5f;
    [Export] private float _acceleration = 0.1f;
    private Node2D? _player;
    private int _direction;

    public override void _Ready() {
        base._Ready();
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _direction = Math.Sign(SpeedX);
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (_player == null) return;

        float playerX = _player.Position.X;
        float missileX = MoveObject.Position.X;

        if (_direction == 1) {
            if (missileX > playerX) {
                if (SpeedX > 0f) SpeedX -= _acceleration;
                else if (SpeedX <= 0f) _direction = -1;
            } else if (SpeedX < _maxSpeedX) {
                SpeedX += _acceleration;
            }
        } else {
            if (missileX < playerX) {
                if (SpeedX < 0f) SpeedX += _acceleration;
                else if (SpeedX >= 0f) _direction = 1;
            } else if (SpeedX > -_maxSpeedX) {
                SpeedX -= _acceleration;
            }
        }
    }
}
