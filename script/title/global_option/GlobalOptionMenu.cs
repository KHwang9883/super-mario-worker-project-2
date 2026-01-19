using Godot;
using System;

public partial class GlobalOptionMenu : Node {
    public enum GlobalOptionMenuEnum {
        GameConfig,
        Controls,
        StaffRoll,
    }
    [Export] public GlobalOptionMenuEnum GlobalOptionMenuType { get; private set; } = GlobalOptionMenuEnum.GameConfig;

    [Export] public Color HighLightColor { get; private set; }
    
    [Export] public Control GameConfigButton { get; private set; } = null!;
    [Export] public Control ControlsButton { get; private set; } = null!;
    [Export] public Control StaffRollButton { get; private set; } = null!;
    
    public override void _Ready() {
        switch (GlobalOptionMenuType) {
            case GlobalOptionMenuEnum.GameConfig:
                GameConfigButton.SelfModulate = HighLightColor;
                break;
            case GlobalOptionMenuEnum.Controls:
                ControlsButton.SelfModulate = HighLightColor;
                break;
            case GlobalOptionMenuEnum.StaffRoll:
                StaffRollButton.SelfModulate = HighLightColor;
                break;
        }
    }

    public void JumpToScene(String sceneUid) {
        // 目标和现场景相同则不跳转
        var resourceCurrent = GD.Load<Resource>(GetTree().CurrentScene.SceneFilePath);
        var resourceTarget = GD.Load<Resource>(sceneUid);
        if (resourceCurrent == resourceTarget) return;
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
