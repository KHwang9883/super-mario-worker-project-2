using Godot;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithStar : Node, IStarHittable {
    [Signal]
    public delegate void StarmanHitEventHandler();
    [Signal]
    public delegate void StarmanHitAddScoreEventHandler(int score);

    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        MetadataInject(_parent);
    }

    [Export] public bool IsStarHittable { get; set; } = true;

    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStar", this);
    }
    public void OnStarmanHit(int score) {
        EmitSignal(SignalName.StarmanHit);
        EmitSignal(SignalName.StarmanHitAddScore, score);
    }
}