using Godot;

namespace SMWP.Level.Block;

[GlobalClass]
public partial class BlockHit : Node {
    [Signal]
    public delegate void BlockBumpEventHandler();
    [Export] protected PackedScene BlockFragmentScene = GD.Load<PackedScene>("uid://c14kue38e0gnl");

    protected Node2D Parent = null!;
    private bool _bump;

    public override void _Ready() {
        Parent = GetParent<Node2D>();
    }
    public virtual void OnBlockHit(Node2D collider) {
        if (!_bump && IsHittable(collider)) {
            _bump = true;
            EmitSignal(SignalName.BlockBump);
        }
    }
    protected virtual bool IsHittable(Node2D collider) {
        return true;
    }
    public virtual void OnBumped() {
        _bump = false;
    }
}