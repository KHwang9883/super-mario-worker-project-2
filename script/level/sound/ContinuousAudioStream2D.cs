using Godot;

namespace SMWP.Level.Sound;

[GlobalClass]
public partial class ContinuousAudioStream2D : AudioStreamPlayer2D {
    private Node? _parent;
    private Viewport? _viewport;
    private bool _playingDetect;
    
    public ContinuousAudioStream2D() {
        ProcessMode = ProcessModeEnum.Always;
        MaxDistance = 99999999999;
        MaxPolyphony = 10;
        Attenuation = 0.06f;
    }
    public override void _Ready() {
        _parent ??= GetParent();
        _viewport ??= GetViewport();
        _parent.TreeExiting += OnParentExiting;
    }
    public override void _PhysicsProcess(double delta) {
        // 若不使用空方法那么从上方踩踏静止龟壳会没有音效，原因不明
        
        if (_playingDetect && !Playing) QueueFree();
    }
    public void OnParentExiting() {
        //Reparent(_viewport);
        
        _parent?.RemoveChild(this);
        Callable.From(() => {
            _viewport?.AddChild(this);
            _playingDetect = true;
        }).CallDeferred();
        Finished += QueueFree;
    }
}
