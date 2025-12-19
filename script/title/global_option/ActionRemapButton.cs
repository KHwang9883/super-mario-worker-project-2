using Godot;
using System;
using System.Collections.Generic;

public partial class ActionRemapButton : SmwpButton {
    [Export] public string Action = null!;
    private bool _allowSetting;
    private bool _setting;

    public override void _Ready() {
        DisplayInputSetting();
    }

    public void OnButtonPressed() {
        //GD.Print("Focused.");
        if (_setting) return;
        Text = "Press a key...".ToUpper();
        ReleaseFocus();
        _allowSetting = true;
    }

    public override void _Input(InputEvent @event) {
        if (_setting && !Input.IsAnythingPressed()) {
            _setting = false;
        }
        if (!_allowSetting) return;
        if (!Input.IsAnythingPressed()) return;
        
        _allowSetting = false;
        _setting = true;
        
        // 键盘等设备按键设置
        var events = InputMap.ActionGetEvents(Action);
        if (@event is not InputEventJoypadButton or InputEventJoypadMotion) {
            InputMap.ActionEraseEvents(Action);
            InputMap.ActionAddEvent(Action, @event);
            if (events.Count > 1) {
                InputMap.ActionAddEvent(Action, events[1]);
            }
        }
        // 手柄按键设置
        else {
            InputMap.ActionEraseEvents(Action);
            InputMap.ActionAddEvent(Action, events[0]);
            if (events.Count > 1) {
                InputMap.ActionAddEvent(Action, @event);
            }
        }

        DisplayInputSetting();
        
        ConfigManager.SmwpConfig.SetValue("control_config", Action, InputMap.ActionGetEvents(Action));
        GrabFocus();
    }
    public void DisplayInputSetting() {
        var events = InputMap.ActionGetEvents(Action);
        List<string> keyboardInputs = new List<string>();
        List<string> gamepadInputs = new List<string>();

        foreach (var inputEvent in events) {
            string text = "";
            
            if (inputEvent is InputEventJoypadButton joyButton) {
                // 直接使用按钮索引，不调用AsText()避免中文
                text = GetGamepadButtonSymbol(joyButton.ButtonIndex);
                gamepadInputs.Add($"({text})");
            }
            else if (inputEvent is InputEventJoypadMotion joyMotion) {
                // 处理摇杆输入
                text = GetAxisSymbol(joyMotion.Axis);
                gamepadInputs.Add($"({text})");
            }
            else {
                text = inputEvent.AsText().ToUpper();
                keyboardInputs.Add(text);
            }
        }

        // 构建显示文本
        if (keyboardInputs.Count > 0 && gamepadInputs.Count > 0) {
            Text = $"{string.Join(" + ", keyboardInputs)} + {string.Join(" + ", gamepadInputs)}";
        }
        else if (keyboardInputs.Count > 0) {
            Text = string.Join(" + ", keyboardInputs);
        }
        else if (gamepadInputs.Count > 0) {
            Text = string.Join(" + ", gamepadInputs);
        }
        else {
            Text = "";
        }
        Text = Text.Replace("(PHYSICAL)", "");

        // 手柄按钮符号映射
        string GetGamepadButtonSymbol(JoyButton button) {
            switch (button) {
                case JoyButton.A: return "A";
                case JoyButton.B: return "B";
                case JoyButton.X: return "X";
                case JoyButton.Y: return "Y";
                case JoyButton.Back: return "SELECT";
                case JoyButton.Start: return "START";
                case JoyButton.Guide: return "HOME";
                case JoyButton.LeftStick: return "LS";
                case JoyButton.RightStick: return "RS";
                case JoyButton.LeftShoulder: return "LB";
                case JoyButton.RightShoulder: return "RB";
                case JoyButton.DpadUp: return "UP";
                case JoyButton.DpadDown: return "DOWN";
                case JoyButton.DpadLeft: return "LEFT";
                case JoyButton.DpadRight: return "RIGHT";
                default: return $"BTN{(int)button}";
            }
        }

        // 摇杆轴符号映射
        string GetAxisSymbol(JoyAxis axis) {
            switch (axis) {
                case JoyAxis.LeftX: return "LS-X";
                case JoyAxis.LeftY: return "LS-Y";
                case JoyAxis.RightX: return "RS-X";
                case JoyAxis.RightY: return "RS-Y";
                case JoyAxis.TriggerLeft: return "LT";
                case JoyAxis.TriggerRight: return "RT";
                default: return $"AXIS{(int)axis}";
            }
        }
    }
}
