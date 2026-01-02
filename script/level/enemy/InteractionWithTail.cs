using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithTail : Node, IRaccoonTailHittable {
    [Signal]
    public delegate void RaccoonTailHitEventHandler();
    
    [Export] public bool IsRaccoonTailHittable { get; set; }
    [Export] public bool ImmuneToTail { get; set; }
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithTail", this);
    }
    public virtual bool OnRaccoonTailHit(Node2D tail) {
        EmitSignal(SignalName.RaccoonTailHit);
        return !ImmuneToTail;
    }
}
