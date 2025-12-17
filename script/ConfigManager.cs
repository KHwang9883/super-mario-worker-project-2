using Godot;
using System;
using System.IO;
using Godot.Collections;
using SMWP;

public static class ConfigManager {
    public static ConfigFile SmwpConfig = new ConfigFile();
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
        
        // Todo: InitialLivesOfSingleLevel
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
        
        // Todo: IsGodMode
        
        // Todo: ControlConfig
        // 获取所有的 Action 的名称
        var actions = InputMap.GetActions();
        // Todo: 摆了（（（
        /*Array<InputMap> defaultActions = null;
        // 获取所有 Action 的第一个按键（键盘默认按键）
        foreach (var action in actions) {
            defaultActions.Add(InputMap.ActionGetEvents(action)[0]);
        }
        foreach (string actionStringName in defaultActions)) {
            string action = actionStringName.new();
            InputMap.ActionEraseEvent(action);
            InputMap.ActionEraseEvent(action, SmwpConfig.GetValue("control_config", action));
        }*/
    }

    public static void SaveConfig() {
        // 保存配置
        Callable.From(() => {
            SmwpConfig.Save(ConfigPath);
        }).CallDeferred();
    }
}
