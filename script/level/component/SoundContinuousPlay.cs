using Godot;

namespace SMWP.Level.Component;

public partial class SoundContinuousPlay : AudioStreamPlayer2D {
    private Node? _parentNode = null!;
    public override void _Ready() {
        _parentNode = GetParent();
        _parentNode.TreeExiting += Reparent;
    }
    public void Reparent() {
        Reparent(GetViewport());
        Finished += QueueFree;
    }
}
