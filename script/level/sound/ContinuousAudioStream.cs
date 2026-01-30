using Godot;
using System;

[GlobalClass]
public partial class ContinuousAudioStream : AudioStreamPlayer {
    private Node? _parent;
    private Viewport? _viewport;
    private bool _playingDetect;
    
    public override void _Ready() {
        _parent ??= GetParent();
        _viewport ??= GetViewport();
        _parent.TreeExiting += OnParentExiting;
    }
    public override void _PhysicsProcess(double delta) {
        // 若不使用空方法那么从上方踩踏静止龟壳会没有音效，原因不明
        
        // 延迟一帧检测，不播放音效的节点不滞留在 Root 下，跟随父节点销毁
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
