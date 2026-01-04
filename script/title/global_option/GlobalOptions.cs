using Godot;
using System;
using SMWP;
using SMWP.Util;

public partial class GlobalOptions : Control {
    [Signal]
    public delegate void ColorAssistHintEventHandler();
    
    [Export] public NodePath InitialFocusButton = null!;
    
    [Export] public Button InitialLivesButton = null!;
    [Export] public LineEdit InitialLivesEdit = null!;
    
    [Export] public Button TemporaryFilesButton = null!;
    [Export] public LineEdit TemporaryFilesEdit = null!;
    
    [Export] public Button DisplayModeButton = null!;
    
    [Export] public Button CustomMusicButton = null!;
    [Export] public LineEdit CustomMusicEdit = null!;
    
    [Export] public Button ColorAssistButton = null!;

    [Export] public Button GodModeButton = null!;

    [Export] public Button ShowFps = null!;
    
    public override void _Ready() {
        // 首先聚焦的选项
        if (!LastInputDevice.IsMouseLastInputDevice()) {
            GetNode<SmwpButton>(InitialFocusButton).CallDeferred("grab_focus");
        }

        OptionDisplayUpdate();
        
        LineEditUpdate();
    }
    public override void _Process(double delta) {
        // 选项显示更新
        OptionDisplayUpdate();
    }

    public void OptionDisplayUpdate() {
        InitialLivesButton.Text = GameManager.InitialLivesOfSingleLevel.ToString();
        TemporaryFilesButton.Text = GameManager.TemporaryFiles.ToString();
        DisplayModeButton.Text =
            DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed
                ? "Windowed".ToUpper()
                : "Fullscreen".ToUpper();
        CustomMusicButton.Text = GameManager.CustomBgmPackage.ToUpper();
        ColorAssistButton.Text = (GameManager.IsColorAccessibilityMode ? "Yes" : "No").ToUpper();
        GodModeButton.Text = (GameManager.IsGodMode ? "Yes" : "No").ToUpper();
        ShowFps.Text = (GameManager.ShowFps ? "Yes" : "No").ToUpper();
    }
    
    public void SetInitialLives() {
        GameManager.InitialLivesOfSingleLevel = InitialLivesEdit.Text.ToInt();
        ConfigManager.SmwpConfig.SetValue("game_config", "initial_lives", GameManager.InitialLivesOfSingleLevel);
        LineEditUpdate();
    }
    public void SetTemporaryFiles() {
        GameManager.TemporaryFiles = TemporaryFilesEdit.Text.ToInt();
        ConfigManager.SmwpConfig.SetValue("game_config", "temporary_files", GameManager.TemporaryFiles);
        LineEditUpdate();
    }
    public void SetDisplayMode() {
        DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
        ConfigManager.SmwpConfig.SetValue("game_config", "display_mode", Variant.From(DisplayServer.WindowGetMode()));
    }
    public void SetCustomMusicPackage() {
        GameManager.CustomBgmPackage = CustomMusicEdit.Text;
        ConfigManager.SmwpConfig.SetValue("game_config", "custom_music_package", GameManager.CustomBgmPackage);
        LineEditUpdate();
    }
    public void SetAccessibilityMode() {
        GameManager.IsColorAccessibilityMode = !GameManager.IsColorAccessibilityMode;
        if (GameManager.IsColorAccessibilityMode) {
            EmitSignal(SignalName.ColorAssistHint);
        }
        ConfigManager.SmwpConfig.SetValue("game_config", "color_assist", GameManager.IsColorAccessibilityMode);
    }
    public void SetPlayLevelInGodMode() {
        GameManager.IsGodMode = !GameManager.IsGodMode;
        ConfigManager.SmwpConfig.SetValue("temporary_test_level", "play_level_in_god_mode", GameManager.IsGodMode);
    }
    public void SetShowFps() {
        GameManager.ShowFps = !GameManager.ShowFps;
        ConfigManager.SmwpConfig.SetValue("game_config", "show_fps", GameManager.ShowFps);
    }

    // 通过弹窗输入值的选项要让弹窗 LineEdit 更新一次
    public void LineEditUpdate() {
        InitialLivesEdit.Text = GameManager.InitialLivesOfSingleLevel.ToString();
        TemporaryFilesEdit.Text = GameManager.TemporaryFiles.ToString();
        CustomMusicEdit.Text = GameManager.CustomBgmPackage;
    }
    
    public void OnBackButtonPressed() {
        // 返回时保存配置文件
        ConfigManager.SaveConfig();
    }
}
