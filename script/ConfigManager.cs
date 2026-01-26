using Godot;
using System;
using System.IO;
using Godot.Collections;
using SMWP;

public static class ConfigManager {
    public static ConfigFile SmwpConfig = new();
    public static string? ConfigDirectory;
    public static readonly string ConfigFileName = "game_settings.cfg";
    public static string? ConfigPath;

    // 初始化获取运行目录
    static ConfigManager() {
        ConfigDirectory = Path.GetDirectoryName(OS.GetExecutablePath()) + "/";
        ConfigDirectory = ConfigDirectory.Replace("\\", "/");
        //GD.Print($"ConfigDirectory: {ConfigDirectory}");
        ConfigPath = ConfigDirectory + ConfigFileName;
        //GD.Print($"ConfigPath: {ConfigPath}");
    }
    
    public static void LoadConfig() {
        // 读取配置
        SmwpConfig.Load(ConfigPath);
        
        GameManager.InitialLivesOfSingleLevel =
            (int)SmwpConfig.GetValue("game_config", "initial_lives", 4);
        
        // Todo: TemporaryFiles
        GameManager.TemporaryFiles =
            (int)SmwpConfig.GetValue("game_config", "temporary_files", 100);
        
        DisplayServer.WindowSetMode(
            SmwpConfig.GetValue("game_config", "display_mode", 
                Variant.From(DisplayServer.WindowMode.Windowed)).As<DisplayServer.WindowMode>()
            );
        
        GameManager.CustomBgmPackage =
            (string)SmwpConfig.GetValue("game_config", "custom_music_package", "Example");
        
        GameManager.IsColorAccessibilityMode =
            (bool)SmwpConfig.GetValue("game_config", "color_assist", false);

        LoadGodMode();
        
        GameManager.UnfocusPause = (bool)SmwpConfig.GetValue("game_config", "unfocus_pause", false);
        
        GameManager.FpsMode = (GameManager.FpsModeEnum)(int)SmwpConfig.GetValue("game_config", "framerate", Variant.From(GameManager.FpsMode == GameManager.FpsModeEnum.F120));
        
        GameManager.ShowFps = (bool)SmwpConfig.GetValue("game_config", "show_fps", false);
        
        GameManager.LoadCurrentDir = (string)SmwpConfig.GetValue(
            "misc", "load_current_dir", OS.GetExecutablePath().GetBaseDir()
            );
        
        // Control Config
        var actions = InputMap.GetActions();
        foreach (var action in actions) {
            if (!SmwpConfig.HasSectionKey("control_config", action)) continue;
            InputMap.ActionEraseEvents(action);
            var events = (Array<InputEvent>)SmwpConfig.GetValue("control_config", action);
            foreach (var @event in events) {
                InputMap.ActionAddEvent(action, @event);
            }
        }
    }

    public static void LoadGodMode() {
        // IsGodMode: Temp for current version without editor part
        GameManager.IsGodMode = (bool)SmwpConfig.GetValue("temporary_test_level", "play_level_in_god_mode", false);
    }
    
    public static void SaveConfig() {
        // 保存配置
        Callable.From(() => {
            SmwpConfig.Save(ConfigPath);
        }).CallDeferred();
    }
}
