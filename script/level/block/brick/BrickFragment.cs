using Godot;
using System;

namespace SMWP.Level.Block.Brick;

public partial class BrickFragment : Node2D {
    [Export] private Sprite2D? _sprite;
    private const float FramerateOrigin = 50f;
    public float SpeedX;
    public float SpeedY;
    private float _gravity = 0.5f;
    private RandomNumberGenerator _random = new RandomNumberGenerator();
    private float _angular;
    
    public override void _Ready() {
        //_sprite = GetNode<Sprite2D>("Sprite2D");
        //SpeedY = -_random.RandiRange(1, 2);
        _angular = _random.RandiRange(0, 19) - _random.RandiRange(0, 19);
    }

    public override void _PhysicsProcess(double delta) {
        if (_sprite == null) return;
        Position = new Vector2(
            Position.X + SpeedX, Position.Y + SpeedY);
        SpeedY += _gravity;
        RotationDegrees += _angular;
    }
}

