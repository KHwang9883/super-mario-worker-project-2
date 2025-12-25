using Godot;
using System;
using SMWP.Util;

public partial class ControlSettings : Control {
    [Export] public NodePath InitialFocusButton = null!;

    private GDC.Array<Node>? _setControlNodes;
    
    public override void _Ready() {
        // 首先聚焦的选项
        if (!LastInputDevice.IsMouseLastInputDevice()) {
            GetNode<SmwpButton>(InitialFocusButton).CallDeferred("grab_focus");
        }
        _setControlNodes = GetTree().GetNodesInGroup("set_control");
    }
    
    public void OnBackButtonPressed() {
        // 返回时保存配置文件
        ConfigManager.SaveConfig();
    }

    public void ResetControls() {
        InputMap.LoadFromProjectSettings();
        ConfigManager.SmwpConfig.EraseSection("control_config");
        if (_setControlNodes == null) {
            return;
        }
        foreach (var node in _setControlNodes) {
            if (node is not ActionRemapButton setControlButton) continue;
            setControlButton.DisplayInputSetting();
        }
    }
}
