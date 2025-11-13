using Godot;
using System;
using SMWP.Level.Interface;

[GlobalClass]
public partial class InteractionWithShell : Node, IShellHittable {
    [Signal]
    public delegate void ShellHitEventHandler();
    [Signal]
    public delegate void ShellHitAddScoreEventHandler(int score);
    [Export] public bool IsShellHittable { get; set; } = true;
    [Export] public bool ImmuneToShell { get; set; }
    [Export] public bool HardToShell { get; set; }

    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithShell", this);
    }
    public bool OnShellHit(int score) {
        EmitSignal(SignalName.ShellHit);
        EmitSignal(SignalName.ShellHitAddScore, score);
        // 被硬物件反死
        return HardToShell;
    }
}
