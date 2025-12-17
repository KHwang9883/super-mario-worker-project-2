using Godot;
using System;

public partial class GlobalOptionMenu : Node {
    public void JumpToScene(String sceneUid) {
        // 目标和现场景相同则不跳转
        var resourceCurrent = GD.Load<Resource>(GetTree().CurrentScene.SceneFilePath);
        var resourceTarget = GD.Load<Resource>(sceneUid);
        if (resourceCurrent == resourceTarget) return;
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
