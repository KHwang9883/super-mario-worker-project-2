using Godot;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithStar : Node, IStarHittable {
    [Signal]
    public delegate void StarmanHitEventHandler();
    [Signal]
    public delegate void StarmanHitAddScoreEventHandler(int score);
    [Export] public bool IsStarHittable { get; set; } = true;
    [Export] public bool ImmuneToStar { get; set; }
    
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStar", this);
    }
    public bool OnStarmanHit(int score) {
        if (!IsStarHittable) return false;
        if (ImmuneToStar) return false;
        EmitSignal(SignalName.StarmanHit);
        EmitSignal(SignalName.StarmanHitAddScore, score);
        return true;
    }
}