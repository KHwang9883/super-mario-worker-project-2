using Godot;
using System;

public partial class KoopaScroll : Node2D {
    [Export] public float ScrollTriggerDistance = 640f;
    [Export] public float Speed = 1f;
    [Export] public int DefaultBgmId = 202;
    public float ScrollPosX;
    public int Id;

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
        // Todo: Koopa is not linked with Scene Control object
        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        // Default Bgm Id
        if (levelConfig.BgmId == DefaultBgmId) return;
        levelConfig.SetBgm(DefaultBgmId);
        //GD.Print($"Koopa Battle Default Bgm Successfully set: {DefaultBgmId}");
    }
}
