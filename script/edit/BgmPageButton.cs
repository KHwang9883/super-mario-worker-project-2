using Godot;
using System;

public partial class BgmPageButton : SmwpButton {
    [Signal]
    public delegate void BgmPageButtonPressedEventHandler(Control target);

    [Export] private Control _targetPage = null!;

    private bool _selected;

    public override void _Ready() {
        Pressed += OnButtonPressed;
        SelectCheck();
    }
    
    public void OnButtonPressed() {
        EmitSignal(SignalName.BgmPageButtonPressed, _targetPage);
        SelectCheck();
    }

    public void SelectCheck() {
        _selected = _targetPage.Visible;
        SelfModulate = SelfModulate with { B = _selected ? 0f : 1f };
    }
}
