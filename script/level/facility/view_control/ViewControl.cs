using Godot;
using System;

public partial class ViewControl : Node2D {
    [Export] public Rect2 ViewRect;

    private SceneControl? _sceneControl;

    public void SetSceneControl(SceneControl sceneControl) {
        _sceneControl = sceneControl;
    }
    public void SetLevelScene() {
        // 没有与场景控制元件相连
        if (_sceneControl == null) return;
        _sceneControl.SetSceneStatus();
    }
}
