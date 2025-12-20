using Godot;
using System;
using Godot.Collections;

public partial class TroopaFlyYellowMovement : Node {
    [Export] public float Radius;
    [Export] public float Angle;
    [Export] public float Direction;
    
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
                _originPos.X + Mathf.Sin(Mathf.DegToRad(Angle)) * Radius,
                _originPos.Y + Mathf.Cos(Mathf.DegToRad(Angle)) * Radius
                );
        
        if (Radius <= 150) {
            Angle -= Direction;
        } else {
            Angle -= (150f / Radius) * Direction;
        }
    }
}
