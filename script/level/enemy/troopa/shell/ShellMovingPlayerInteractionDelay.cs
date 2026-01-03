using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Troopa.Shell;

public partial class ShellMovingPlayerInteractionDelay : Node {
    [Export] private int _delay = 10;
    [Export] private EnemyInteraction _enemyInteractionComponent = null!;

    private CharacterBody2D _parent = null!;
    private uint _originCollisionLayer;

    public override void _Ready() {
        _parent = GetParent<CharacterBody2D>();
        _originCollisionLayer = _parent.CollisionLayer;
        _parent.CollisionLayer = 0;
    }

    public override void _PhysicsProcess(double delta) {
        if (_delay <= 0) return;
        _delay--;
        if (_delay > 0) return;
        /*
        _enemyInteractionComponent.HurtType = IHurtableAndKillable.HurtEnum.Hurt;
        */
        _parent.CollisionLayer = _originCollisionLayer;
    }

    public void SetDelay(int delay) {
        _delay = delay;
    }
}
