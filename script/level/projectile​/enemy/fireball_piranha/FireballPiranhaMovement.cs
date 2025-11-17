using Godot;
using System;
using SMWP.Level.Physics;

public partial class FireballPiranhaMovement : BasicMovement {
    [Export] private Sprite2D _sprite2D = null!;
    [Export] private float _extraSpeedY = -6f;
    private RandomNumberGenerator _rng = new();
    
    public override void _Ready() {
        base._Ready();
        SpeedX = _rng.RandiRange(0, 6) - _rng.RandiRange(0, 6);
        if (!MoveObject.HasMeta("PiranhaPlantAngle")) return;
        var angle = (float)MoveObject.GetMeta("PiranhaPlantAngle");
        SpeedY += _extraSpeedY * Mathf.Cos(angle);
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        _sprite2D.RotationDegrees += 30f;
    }
}
