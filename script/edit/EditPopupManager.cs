using Godot;
using System;

public partial class EditPopupManager : Node {
    [Signal] public delegate void TextChangedEventHandler();
    
    [Export] public Label PopupLabel = null!;

    public void SetPopupText(string text) {
        PopupLabel.Text = text;
        EmitSignal(SignalName.TextChanged);
    }
}
