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
        //Reparent(_viewport);
        _parent.RemoveChild(this);
        Callable.From(() => {
            if (_viewport != null) _viewport.AddChild(this);
        }).CallDeferred();
        Finished += QueueFree;
    }
}
