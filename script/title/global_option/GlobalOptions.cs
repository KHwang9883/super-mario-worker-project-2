using Godot;
using System;

public partial class GlobalOptions : Control {
    [Export] public Button DisplayModeButton = null!;
    public override void _Ready() {
        GetNode<SmwpButton>("VBoxContainerOptions/InitialLives/SetInitialLives").CallDeferred("grab_focus");
    }

    public void SetInitialLives() {
        // Todo: 单关初始自定义命数配置
    }
    public void SetTemporaryFiles() {
        // Todo: 测试文件数配置
    }
    public void OnDisplayModePressed() {
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen) {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            DisplayModeButton.Text = "Fullscreen".ToUpper();
        } else {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayModeButton.Text = "Windowed".ToUpper();
        }
    }
    public void SetCustomMusicPackage() {
        // Todo: 自定义音乐包配置
    }
    public void SetAccessibilityMode() {
        // Todo: 颜色辅助功能模式配置
    }
}
