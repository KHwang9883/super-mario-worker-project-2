using Godot;
using System;

public partial class Thunder : Node2D {
    private RandomNumberGenerator _rng = new();
    
    public override void _Ready() {
        Position = new Vector2(_rng.RandfRange(64, 640 - 64), -16f);
        RotationDegrees = _rng.RandfRange(0, 120) - 60;
        Scale *= (80f + _rng.RandfRange(0f, 70f)) / 100f;
    }
}
