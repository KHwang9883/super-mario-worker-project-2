using Godot;
using System;

public partial class FireballExplosion : Node2D {
    private Sprite2D _sprite = null!;

    public override void _Ready() {
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _PhysicsProcess(double delta) {
        _sprite.Scale = new Vector2(_sprite.Scale.X - 0.05f, _sprite.Scale.Y - 0.05f);
        _sprite.Modulate = _sprite.Modulate with { A = _sprite.Modulate.A - 0.05f };
        if (_sprite.Modulate.A <= 0f) {
            QueueFree();
        }
    }
}
