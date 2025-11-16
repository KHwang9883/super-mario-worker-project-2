using Godot;
using System;
using Godot.Collections;

public partial class TroopaFlyYellowMovement : Node {
    [Export] private float _radius;
    [Export] private float _angle;
    [Export] private float _direction;
    
    private Node2D? _parent;
    private Vector2 _originPos;
    
    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _originPos = _parent.Position;
    }

    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;
        
        _parent.Position =
            new Vector2(
                _originPos.X + Mathf.Sin(Mathf.DegToRad(_angle)) *_radius,
                _originPos.Y + Mathf.Cos(Mathf.DegToRad(_angle)) *_radius
                );
        
        if (_radius <= 150) {
            _angle -= _direction * 2f - 1f;
        } else {
            _angle -= (150f / _radius) * (_direction * 2f - 1f);
        }
    }
}
