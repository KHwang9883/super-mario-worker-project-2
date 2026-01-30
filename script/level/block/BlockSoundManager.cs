using Godot;
using System;
using SMWP.Level.Block;

[GlobalClass]
public partial class BlockSoundManager : Node {
    [Export] public NodePath PathToBlockHit { get; private set; } = "../BlockHit";
    
    [Export] public bool PlaySoundBreak { get; private set; } = true;
    [Export] public bool PlaySoundBump { get; private set; } = true;
    
    private BlockHit _blockHit = null!;
    public AudioStreamPlayer SoundStomped { get; private set; } = null!;
    public AudioStreamPlayer SoundKicked { get; private set; } = null!;

    public override void _Ready() {
        base._Ready();
        _blockHit = GetNode<BlockHit>(PathToBlockHit);
        SoundStomped = GetNode<AudioStreamPlayer>("Break");
        SoundKicked = GetNode<AudioStreamPlayer>("Bump");
        if (PlaySoundBreak) _blockHit.BlockBreak += PlayBreak;
        if (PlaySoundBump) _blockHit.BlockBump += PlayBump;
    }

    public override void _ExitTree() {
        if (PlaySoundBreak) _blockHit.BlockBreak -= PlayBreak;
        if (PlaySoundBump) _blockHit.BlockBump -= PlayBump;
    }
    
    public void PlayBreak() {
        SoundStomped.Play();
    }
    public void PlayBump() {
        SoundKicked.Play();
    }
}
