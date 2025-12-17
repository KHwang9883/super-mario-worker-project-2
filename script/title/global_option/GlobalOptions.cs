using Godot;
using System;
using SMWP;

public partial class GlobalOptions : Control {
    [Export] public Button InitialLivesButton = null!;
    [Export] public Button TemporaryFilesButton = null!;
    [Export] public Button DisplayModeButton = null!;
    [Export] public Button CustomMusicButton = null!;
    [Export] public Button ColorAssistButton = null!;
    
    public override void _Ready() {
        GetNode<SmwpButton>("VBoxContainerOptions/InitialLives/SetInitialLives").CallDeferred("grab_focus");
    }

    public override void _Process(double delta) {
        // 选项显示
        InitialLivesButton.Text = GameManager.InitialLivesOfSingleLevel.ToString();
        TemporaryFilesButton.Text = GameManager.TemporaryFiles.ToString();
        DisplayModeButton.Text =
            DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed
                ? "Windowed".ToUpper()
                : "Fullscreen".ToUpper();
        CustomMusicButton.Text = GameManager.CustomBgmPackage.ToUpper();
        ColorAssistButton.Text = (GameManager.IsColorAccessibilityMode ? "Yes" : "No").ToUpper();
    }

    public void SetInitialLives() {
        // Todo: 单关初始自定义命数配置
        // Todo: Get Lives int from popup
        ConfigManager.SmwpConfig.SetValue("game_config", "initial_lives", GameManager.InitialLivesOfSingleLevel);
    }
    public void SetTemporaryFiles() {
        // Todo: 测试文件数配置
        // Todo: Get Temp Files int from popup
        ConfigManager.SmwpConfig.SetValue("game_config", "temporary_files", GameManager.TemporaryFiles);
    }
    public void OnDisplayModePressed() {
        DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
        ConfigManager.SmwpConfig.SetValue("game_config", "display_mode", Variant.From(DisplayServer.WindowGetMode()));
    }
    public void SetCustomMusicPackage() {
        // Todo: 自定义音乐包配置
        // Todo: Get Package string from popup
        ConfigManager.SmwpConfig.SetValue("game_config", "custom_music_package", GameManager.CustomBgmPackage);
    }
    public void SetAccessibilityMode() {
        GameManager.IsColorAccessibilityMode = !GameManager.IsColorAccessibilityMode;
        ConfigManager.SmwpConfig.SetValue("game_config", "color_assist", GameManager.IsColorAccessibilityMode);
    }

    public void OnBackButtonPressed() {
        // 返回时保存配置文件
        ConfigManager.SaveConfig();
    }
}
