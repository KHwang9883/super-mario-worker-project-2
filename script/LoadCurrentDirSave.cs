using Godot;
using System;
using SMWP;

public partial class LoadCurrentDirSave : FileDialog {
    public override void _Ready() {
        CurrentDir = GameManager.LoadCurrentDir;
        Confirmed += SaveCurrentDir;
        FileSelected += SaveCurrentDir;
    }

    public void SaveCurrentDir(string nothing) {
        SaveCurrentDir();
    }
    public void SaveCurrentDir() {
        GameManager.LoadCurrentDir = GetCurrentDir();
        ConfigManager.SmwpConfig.SetValue("misc",  "load_current_dir", CurrentDir);
        ConfigManager.SaveConfig();
    }
}
