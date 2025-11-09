using Godot;
using System;

public partial class PlayerInvincibleEffect : AnimatedSprite2D {
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    private static readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready() {
        _animatedSprite2D = GetParent().GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
    }
    public override void _PhysicsProcess(double delta) {
        if (_animatedSprite2D == null) return;
        if (!Visible) return;
        SpriteFrames = _animatedSprite2D.SpriteFrames;
        Animation = _animatedSprite2D.Animation;
        Frame = _animatedSprite2D.Frame;
        FlipH = _animatedSprite2D.FlipH;
        Modulate = new Color(_rng.Randf(), _rng.Randf(), _rng.Randf(), 1.0f);
    }
}
