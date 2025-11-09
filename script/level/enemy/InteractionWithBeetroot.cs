using Godot;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithBeetroot : Node, IBeetrootHittable {
    [Signal]
    public delegate void BeetrootHitEventHandler();

    [Export] public bool IsBeetrootHittable { get; set; } = true;
    [Export] public bool BeetrootBump { get; set; }
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithBeetroot", this);
    }
    public virtual bool OnBeetrootHit(Node2D beetroot) {
        EmitSignal(SignalName.BeetrootHit);
        return BeetrootBump;
    }
}