using Godot;
using System;

public partial class ShellSoundManager : EnemySoundManager {
    [Export] public NodePath PathToShellInteraction = "../ShellInteraction";

    public override void _Ready() {
        base._Ready();
        var shellInteraction = GetNode<ShellMovingInteraction>(PathToShellInteraction);
        shellInteraction.Harded += PlayKicked;
    }
}
