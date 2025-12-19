using Godot;
using System;

public partial class KoopaScroll : Node2D {
    [Export] public float ScrollTriggerDistance = 640f;
    [Export] public float Speed = 1f;
    [Export] public int DefaultBgmId = 202;

    // 用于判定连接的场景控制元件是否在要求范围内
    [Export] public Marker2D SceneMarkerPos = null!;
    [Export] public Marker2D SceneMarkerSize = null!;
    
    public float ScrollPosX;
    public int Id;
    
    private SceneControl? _sceneControl;

    public override void _Ready() {
        ScrollPosX = GlobalPosition.X;
    }
    /*public override void _PhysicsProcess(double delta) {
        if (IsInGroup("koopa_scroll")) {
            GD.Print($"{this} is in group.");
        }
    }*/

    public override void _ExitTree() {
        RemoveFromGroup("koopa_scroll");
    }

    public void SetBgm() {
        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        // Default Bgm Id
        if (levelConfig.BgmId == DefaultBgmId) return;
        levelConfig.SetBgm(DefaultBgmId);
        //GD.Print($"Koopa Battle Default Bgm Successfully set: {DefaultBgmId}");
    }
    
    public void SetSceneControl(SceneControl sceneControl) {
        _sceneControl = sceneControl;
    }
    public void SetLevelScene() {
        // 没有与场景控制元件相连
        if (_sceneControl == null) return;
        _sceneControl.SetSceneStatus();
    }
}
