using Godot;
using System;

public partial class FireballSprite : Node {
    [Export] private Sprite2D _sprite = null!;
    [Export] private FireballMovement _fireballMovement = null!;

    public override void _PhysicsProcess(double delta) {
        _sprite.RotationDegrees -= 10f * _fireballMovement.Direction;
    }
}
