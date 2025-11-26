using Godot;
using System;

public partial class GeneralVisibleOnScreenEnabler2d : VisibleOnScreenNotifier2D {
    [Export] public NodePath Path = "..";
    [Export] public bool DisableWhenOutOfScreen;

    public void OnScreenEntered() {
        GetNode(Path).ProcessMode = ProcessModeEnum.Inherit;
        if (!DisableWhenOutOfScreen) QueueFree();
    }
    public void OnScreenExited() {
        GetNode(Path).ProcessMode = ProcessModeEnum.Disabled;
    }
}
