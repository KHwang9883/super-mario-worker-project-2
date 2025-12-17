using Godot;
using System;

public partial class ControlSettings : Control {
    public void OnBackButtonPressed() {
        // 返回时保存配置文件
        ConfigManager.SaveConfig();
    }
}
