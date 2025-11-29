using Godot;
using System;

public partial class GlobalOptions : Control {
    public override void _Ready() {
        GetNode<SmwpButton>("VBoxContainerLabel/InitialLives/SetInitialLives").CallDeferred("grab_focus");
    }
    
    public void JumpToScene(String sceneUid) {
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
