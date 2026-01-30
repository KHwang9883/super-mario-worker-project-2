using Godot;
using System;
using SMWP.Level.Sound;

public partial class HammerBroShoot : Node {
    [Signal]
    public delegate void PlaySoundBroShootEventHandler();
    
    [Export] private PackedScene _projectileScene = GD.Load<PackedScene>("uid://bcineprwgpk4w");
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    [Export] private float _shootTime = 250f;
    
    private Node2D? _parent;
    private float _shootTimer;
    private float _shootTimerBoost;
    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        _parent ??= GetParent<Node2D>();
        _shootTimerBoost = _rng.RandfRange(0f, 0.2f);
    }
    public override void _PhysicsProcess(double delta) {
        // Shoot Status
        _shootTimer += 1f + _shootTimerBoost;
        if (_shootTimer > _shootTime * 0.6f) {
            if (_animatedSprite2D?.Animation != "shoot") {
                _animatedSprite2D?.Play("shoot");
            }
        }
        if (_shootTimer > _shootTime) {
            Launch();
            _animatedSprite2D?.Play("default");
            EmitSignal(SignalName.PlaySoundBroShoot);
            _shootTimer = 0f;
        }
    }
    
    public void Launch() {
        if (_parent == null) {
            GD.PushError($"{this}: _parent is null!");
            return;
        }
        var projectile = _projectileScene.Instantiate<Node2D>();
        projectile.SetPosition(_parent.Position + Vector2.Up * 10f);
        _parent.AddSibling(projectile);
    }
}
