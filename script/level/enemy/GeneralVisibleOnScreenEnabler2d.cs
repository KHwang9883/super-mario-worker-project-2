using Godot;
using System;

public partial class GeneralVisibleOnScreenEnabler2d : VisibleOnScreenNotifier2D {
    [Export] public NodePath Path = "..";
    [Export] public bool DisableWhenOutOfScreen;

    private Node? _parent;

    public override void _Ready() {
        _parent = GetNode(Path);
    }
    
    public void OnScreenEntered() {
        if (_parent == null) {
            GD.Print("GeneralVisibleOnScreenEnabler2d: _parent is null!");
            return;
        }
        _parent.ProcessMode = ProcessModeEnum.Inherit;
        if (!DisableWhenOutOfScreen) QueueFree();
    }
    public void OnScreenExited() {
        if (_parent == null) {
            GD.Print("GeneralVisibleOnScreenEnabler2d: _parent is null!");
            return;
        }
        _parent.ProcessMode = ProcessModeEnum.Disabled;
    }
}
