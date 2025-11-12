using Godot;
using System;

public partial class TroopaFlyRedMovement : Node {
    [Export] private float _swingSpeed = 0.03f;
    [Export] private float _swingHeight = 50f;
    private Node2D _parent = null!;
    private float _originGlobalPositionY;
    private float _angle;
    
    public override void _Ready() {
        _parent = (Node2D)GetParent();
        _originGlobalPositionY = _parent.GlobalPosition.Y;
    }
    public override void _PhysicsProcess(double delta) {
        _angle += _swingSpeed;
        var phase = Mathf.Cos(Mathf.Wrap(_angle, 0, Mathf.Tau));
        _parent.GlobalPosition = _parent.GlobalPosition with 
            { Y = _originGlobalPositionY + phase * _swingHeight};
    }
}
