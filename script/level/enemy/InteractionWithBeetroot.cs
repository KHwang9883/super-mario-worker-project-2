using Godot;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithBeetroot : Node, IBeetrootHittable {
    [Signal]
    public delegate void BeetrootHitEventHandler();
    [Export] public bool BeetrootBump { get; set; }
    
    public virtual bool OnBeetrootHit(Node2D beetroot) {
        EmitSignal(SignalName.BeetrootHit);
        return BeetrootBump;
    }
}