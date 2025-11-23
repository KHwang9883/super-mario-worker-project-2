using Godot;
using System;

public partial class GeneralVisibleOnScreenEnabler2d : VisibleOnScreenNotifier2D {
    [Export] public NodePath Path = "..";

    public void OnScreenEntered() {
        GetNode(Path).ProcessMode = ProcessModeEnum.Inherit;
    }
    public void OnScreenExited() {
        GetNode(Path).ProcessMode = ProcessModeEnum.Disabled;
    }
}
