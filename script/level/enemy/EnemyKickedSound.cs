using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Sound;

public partial class EnemyKickedSound : ContinuousAudioStream2D {
    [Export] private NodePath _pathToInteraction = "../EnemyInteraction";
    private EnemyInteraction? _enemyInteraction;
    
    public override void _Ready() {
        base._Ready();
        _enemyInteraction = GetNode<EnemyInteraction>(_pathToInteraction);
        _enemyInteraction.RaccoonTailHit += PlaySound;
    }

    public void PlaySound() {
        Play();
    }
}
