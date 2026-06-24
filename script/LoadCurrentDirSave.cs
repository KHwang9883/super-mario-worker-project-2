using Godot;
using System;
using System.Reflection;
using SMWP;

public partial class LoadCurrentDirSave : FileDialog {
    public enum ModeType {
        PlayLoadCurrentDir,
        EditSaveCurrentDir,
        EditLoadCurrentDir,
    }
    [Export] public ModeType DialogMode = ModeType.PlayLoadCurrentDir;
    
    public static readonly GDC.Dictionary<ModeType, string> ConfigKeyNameMap = new GDC.Dictionary<ModeType, string> { 
        { ModeType.PlayLoadCurrentDir, "play_load_current_dir" },
        { ModeType.EditSaveCurrentDir, "edit_save_current_dir" },
        { ModeType.EditLoadCurrentDir, "edit_load_current_dir" },
    }; 
    public string ConfigKeyName = "";

    private string GetGameManagerFieldName() {
        switch (DialogMode) {
            case ModeType.PlayLoadCurrentDir: return nameof(GameManager.PlayLoadCurrentDir);
            case ModeType.EditSaveCurrentDir: return nameof(GameManager.EditSaveCurrentDir);
            case ModeType.EditLoadCurrentDir: return nameof(GameManager.EditLoadCurrentDir);
            default: return nameof(GameManager.PlayLoadCurrentDir);
        }
    }

    private string GetCurrentDirFromGameManager() {
        var field = typeof(GameManager).GetField(
            GetGameManagerFieldName(),
            BindingFlags.Public | BindingFlags.Static
        );
        if (field != null) {
            var value = field.GetValue(null);
            if (value is string str) return str;
        }
        return "";
    }

    private void SetCurrentDirToGameManager(string path) {
        var field = typeof(GameManager).GetField(
            GetGameManagerFieldName(),
            BindingFlags.Public | BindingFlags.Static
        );
        if (field != null) {
            field.SetValue(null, path);
        }
    }

    public override void _Ready() {
        ConfigKeyName = ConfigKeyNameMap[DialogMode];
        CurrentDir = GetCurrentDirFromGameManager();
        Confirmed += SaveCurrentDir;
        FileSelected += SaveCurrentDir;
    }

    public void SaveCurrentDir(string _) {
        SaveCurrentDir();
    }

    public void SaveCurrentDir() {
        SetCurrentDirToGameManager(GetCurrentDir());
        ConfigManager.SmwpConfig.SetValue("misc", ConfigKeyName, CurrentDir);
        ConfigManager.SaveConfig();
    }
}