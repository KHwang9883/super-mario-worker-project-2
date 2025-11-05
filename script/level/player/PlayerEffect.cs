using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerEffect : Sprite2D {
    public override void _PhysicsProcess(double delta) {
        Modulate = Modulate with { A = Modulate.A - 0.2f };
        if (Modulate.A <= 0f) {
            QueueFree();
        }
    }
}
