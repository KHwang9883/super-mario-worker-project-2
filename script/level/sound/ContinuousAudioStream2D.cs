using Godot;

namespace SMWP.Level.Sound;

[GlobalClass]
public partial class ContinuousAudioStream2D : AudioStreamPlayer2D {
    private Node _parent = null!;
    private Viewport _viewport = null!;
    
    public ContinuousAudioStream2D() {
        ProcessMode = ProcessModeEnum.Always;
    }
    public override void _Ready() {
        _parent = GetParent();
        _viewport = GetViewport();
        _parent.TreeExiting += OnParentExiting;
    }
    public void OnParentExiting() {
        Reparent(_viewport);
        Finished += QueueFree;
    }
}
