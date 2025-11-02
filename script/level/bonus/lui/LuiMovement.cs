using Godot;
using System;

public partial class LuiMovement : Node {
    [Export] private Area2D _lui = null!;
    private float _originY;
    private float _speedY = -8f;
    
    public override void _Ready() {
        _originY = _lui.Position.Y;
    }

    public override void _PhysicsProcess(double delta) {
        _lui.Position = new Vector2(_lui.Position.X, _lui.Position.Y + _speedY);
        _speedY += 0.4f;
        if (_speedY > 8f) {
            _lui.Position = new Vector2(_lui.Position.X, _originY);
            _speedY = -8f;
        }
    }
}
