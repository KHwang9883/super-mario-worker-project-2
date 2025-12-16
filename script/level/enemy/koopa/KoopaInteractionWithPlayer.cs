using Godot;
using System;
using SMWP.Level.Enemy;

public partial class KoopaInteractionWithPlayer : InteractionWithPlayer {
    public override void _Ready() {
        base._Ready();
        Parent?.SetMeta("KoopaStomp", this);
    }
}
