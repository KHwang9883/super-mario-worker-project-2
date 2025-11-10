using Godot;
using System;

public partial class EnemyDeadNormalSpin : Node {
    private Node2D? _parent;
    private float _rotationDegreesCount;
    private float _spinSpeed = 10f;
    public override void _Ready() {
        _parent = (Node2D)GetParent();
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;
        if (!(_rotationDegreesCount < 180)) return;
        _rotationDegreesCount += _spinSpeed;
        _parent.RotationDegrees -= _spinSpeed;
    }
}
