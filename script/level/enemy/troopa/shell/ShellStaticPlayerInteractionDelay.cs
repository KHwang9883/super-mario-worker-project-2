using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Troopa.Shell;

public partial class ShellStaticPlayerInteractionDelay : Node {
    [Export] private int _delay = 3;
    [Export] private EnemyInteraction _enemyInteractionComponent = null!;
    
    public override void _PhysicsProcess(double delta) {
        if (_delay <= 0) return;
        _delay--;
        if (_delay > 0) return;
        _enemyInteractionComponent.HurtType = IHurtableAndKillable.HurtEnum.Nothing;
    }
}
