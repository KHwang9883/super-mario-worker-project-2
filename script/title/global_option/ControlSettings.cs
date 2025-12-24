using Godot;
using System;
using SMWP.Util;

public partial class ControlSettings : Control {
    [Export] public NodePath InitialFocusButton = null!;
    
    public override void _Ready() {
        // 首先聚焦的选项
        if (!LastInputDevice.IsMouseLastInputDevice()) {
            GetNode<SmwpButton>(InitialFocusButton).CallDeferred("grab_focus");
        }
    }
    
    public void OnBackButtonPressed() {
        // 返回时保存配置文件
        ConfigManager.SaveConfig();
    }
}
