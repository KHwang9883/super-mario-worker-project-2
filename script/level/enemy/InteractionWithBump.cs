using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithBump : Node, IToppable {
    [Signal]
    public delegate void ToppedEventHandler();
    
    [Export] public bool IsToppable { get; set; } = true;
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithBump", this);
    }
    public virtual void OnTopped() {
        EmitSignal(SignalName.Topped);
    }
}
