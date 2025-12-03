using Godot;
using System;

public partial class JumpToSceneButton : SmwpButton {
    public void JumpToScene(String sceneUid) {
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
