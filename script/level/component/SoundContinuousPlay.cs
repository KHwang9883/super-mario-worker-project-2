using Godot;

namespace SMWP.Level.Component;

public partial class SoundContinuousPlay : AudioStreamPlayer2D {
    private Node? _parent;
    public override void _Ready() {
        _parent ??= GetParent();
        _parent.TreeExiting += Reparent;
    }
    public void Reparent() {
        Reparent(GetViewport());
        Finished += QueueFree;
    }
}
