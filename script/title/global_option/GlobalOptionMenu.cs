using Godot;
using System;

public partial class GlobalOptionMenu : Node {
    public void JumpToScene(String sceneUid) {
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
