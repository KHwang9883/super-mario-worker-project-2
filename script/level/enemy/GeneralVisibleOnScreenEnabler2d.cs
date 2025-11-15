using Godot;
using System;

public partial class GeneralVisibleOnScreenEnabler2d : VisibleOnScreenNotifier2D {
    [Export] private NodePath _path = "..";

    public void OnScreenEntered() {
        GetNode(_path).ProcessMode = ProcessModeEnum.Inherit;
    }
}
