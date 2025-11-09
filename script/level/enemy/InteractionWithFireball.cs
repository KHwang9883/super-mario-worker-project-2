using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithFireball : Node, IFireballHittable {
    [Signal]
    public delegate void FireballHitEventHandler();

    [Export] public bool IsFireballHittable { get; set; } = true;
    [Export] public bool FireballExplode { get; set; } = true;
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithFireball", this);
    }
    public virtual bool OnFireballHit(Node2D fireball) {
        EmitSignal(SignalName.FireballHit);
        return FireballExplode;
    }
}
