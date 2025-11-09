using Godot;
using System;

public partial class ScoreEffect : Sprite2D {
    private int _timer;
    
    public override void _PhysicsProcess(double delta) {
        Position = Position with { Y = Position.Y - 0.5f };
        _timer++;
        if (_timer <= 100) return;
        Modulate = Modulate with { A = Modulate.A - 0.1f };
        if (Modulate.A > 0f) return;
        QueueFree();
    }
}
