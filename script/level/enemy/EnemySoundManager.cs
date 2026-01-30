using Godot;
using System;
using SMWP.Level.Enemy;

[GlobalClass]
public partial class EnemySoundManager : Node {
    [Export] public NodePath PathToEnemyInteraction { get; private set; } = "../EnemyInteraction";
    
    [Export] public bool PlaySoundStomped { get; private set; } = true;
    [Export] public bool PlaySoundKicked { get; private set; } = true;
    [Export] public bool PlaySoundBumped { get; private set; } = true;
    [Export] public bool PlayRaccoonTailHit { get; private set; } = true;
    
    private EnemyInteraction _enemyInteraction = null!;
    public AudioStreamPlayer SoundStomped { get; private set; } = null!;
    public AudioStreamPlayer SoundKicked { get; private set; } = null!;
    public AudioStreamPlayer SoundBumped { get; private set; } = null!;

    public override void _Ready() {
        base._Ready();
        _enemyInteraction = GetNode<EnemyInteraction>(PathToEnemyInteraction);
        SoundStomped = GetNode<AudioStreamPlayer>("Stomped");
        SoundKicked = GetNode<AudioStreamPlayer>("Kicked");
        SoundBumped = GetNode<AudioStreamPlayer>("Bumped");
        if (PlaySoundStomped) _enemyInteraction.PlaySoundStomped += PlayStomped;
        if (PlaySoundKicked) _enemyInteraction.PlaySoundKicked += PlayKicked;
        if (PlaySoundBumped) _enemyInteraction.PlaySoundBumped += PlayBumped;
        if (PlayRaccoonTailHit) _enemyInteraction.RaccoonTailHit += PlayKicked;
    }

    public override void _ExitTree() {
        if (PlaySoundStomped) _enemyInteraction.PlaySoundStomped -= PlayStomped;
        if (PlaySoundKicked) _enemyInteraction.PlaySoundKicked -= PlayKicked;
        if (PlaySoundBumped) _enemyInteraction.PlaySoundBumped -= PlayBumped;
        if (PlayRaccoonTailHit) _enemyInteraction.RaccoonTailHit -= PlayKicked;
    }
    
    public void PlayStomped() {
        SoundStomped.Play();
    }
    public void PlayKicked() {
        SoundKicked.Play();
    }
    public void PlayBumped() {
        SoundBumped.Play();
    }
}
