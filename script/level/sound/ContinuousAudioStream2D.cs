using Godot;

namespace SMWP.Level.Sound;

[GlobalClass]
public partial class ContinuousAudioStream2D : AudioStreamPlayer2D {
    private Node _parent = null!;
    private Viewport? _viewport;
    
    public ContinuousAudioStream2D() {
        ProcessMode = ProcessModeEnum.Always;
        MaxDistance = 99999999999;
        MaxPolyphony = 10;
        Attenuation = 0.06f;
    }
    public override void _Ready() {
        _parent = GetParent();
        _viewport = GetViewport();
        _parent.TreeExiting += OnParentExiting;
    }
    public override void _PhysicsProcess(double delta) {
        // 若不使用空方法那么从上方踩踏静止龟壳会没有音效，原因不明
    }
    public void OnParentExiting() {
        //Reparent(_viewport);
        _parent.RemoveChild(this);
        Callable.From(() => {
            _viewport?.AddChild(this);
        }).CallDeferred();
        Finished += QueueFree;
    }
}
