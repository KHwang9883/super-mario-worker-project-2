using Godot;
using System;
using SMWP.Level;

public partial class CoinEffect : Node2D {
    [Export] private AnimatedSprite2D _ani = null!;
    private float _timer;

    public override void _Ready() {
        LevelManager.AddCoin(1);
    }
    public override void _PhysicsProcess(double delta) {
        if (_timer < 16f) {
            Position -= new Vector2(0f, 14f -_timer);
            _timer += 1f;
        }
        if (_timer >= 16f) {
            Position += new Vector2(0f, 1f);
            _timer += 1f;
            _ani.Modulate =
                _ani.Modulate with { A = Mathf.MoveToward(_ani.Modulate.A, 0f, 0.1f) };
        }
        if (_timer > 26f) {
            QueueFree();
        }
    }
}
