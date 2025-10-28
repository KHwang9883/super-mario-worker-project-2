using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerDead : Node2D {
    private float _gravity = 0.4f;
    private float _speedY = -6f;
    private int _timer;

    public override void _PhysicsProcess(double delta) {
        if (_timer < 50) {
            _timer++;
        } else {
            Position = new Vector2(Position.X, Position.Y + _speedY);
            if (_speedY < 10f) {
                _speedY += _gravity;
            }
        }
    }
}
