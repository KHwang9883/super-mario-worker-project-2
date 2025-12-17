using Godot;
using System;

public partial class ActionRemapButton : SmwpButton {
    [Export] public string Action = null!;

    public override void _Toggled(bool toggledOn) {
        base._Toggled(toggledOn);
        if (toggledOn) {
            
        } else {
            
        }
    }
}
