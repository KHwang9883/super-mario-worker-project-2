using Godot;
using System;
using SMWP.Level.Enemy;

public partial class KoopaInteractionWithPlayer : InteractionWithPlayer {
    private bool _isKoopaHurt;
    
    public override void _Ready() {
        base._Ready();
        Parent?.SetMeta("KoopaStomp", this);
    }
    public override void _PhysicsProcess(double delta) {
        Stompable = !_isKoopaHurt;
    }

    public void OnHurtStarted() {
        _isKoopaHurt = true;
    }
    public void OnHurtEnded() {
        _isKoopaHurt = false;
    }
}
