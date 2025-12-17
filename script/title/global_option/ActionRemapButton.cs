using Godot;
using System;

public partial class ActionRemapButton : SmwpButton {
    [Export] public string Action = null!;
    private bool _settingKey;
    private InputEvent? _defaultAction;

    public override void _Ready() {
        Text = InputMap.GetActionDescription(Action)
            .ToUpper()
            .Replace("(PHYSICAL)", "")
            .Trim();
        _defaultAction = InputMap.ActionGetEvents(Action)[0];
    }

    public void OnButtonPressed() {
        //GD.Print("Focused.");
        Text = "Press a key...".ToUpper();
        ReleaseFocus();
        _settingKey = true;
    }

    public override void _Input(InputEvent @event) {
        if (!_settingKey) return;
        if (!Input.IsAnythingPressed()) return;
        
        //GD.Print("Key set.");
        _settingKey = false;
        InputMap.ActionEraseEvent(Action, _defaultAction);
        InputMap.ActionAddEvent(Action, @event);
        Text = @event.AsText().ToUpper();
        ConfigManager.SmwpConfig.SetValue("control_config", Action, @event);
        GrabFocus();
    }
}
