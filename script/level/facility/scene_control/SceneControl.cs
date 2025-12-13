using Godot;
using System;
using SMWP.Level;

public partial class SceneControl : Area2D {
    public enum LinkedWithObjectEnum {
        None,
        ViewControl,
        Koopa,
    }
    [Export] public LinkedWithObjectEnum LinkedWithObject = LinkedWithObjectEnum.None;
    [Export] public bool ChangeBgm;
    [Export] public int BgmId = 1;
    [Export] public bool ChangeBgp;
    [Export] public int BgpId = 1;
    [Export] public float WaterHeight = -64f;
    [Export] public bool ChangeWeather;
    [Export] public int RainyLevel;
    [Export] public int FallingStarsLevel;
    [Export] public int SnowyLevel;
    [Export] public int ThunderLevel;
    [Export] public int WindyLevel;
    [Export] public int Darkness;
    [Export] public int Brightness;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);

        switch (LinkedWithObject) {
            case LinkedWithObjectEnum.None:
                break;
            case LinkedWithObjectEnum.ViewControl:
                LinkWithViewControl();
                break;
            case LinkedWithObjectEnum.Koopa:
                LinkWithKoopa();
                break;
        }
    }
    
    public void OnBodyEntered(Node2D body) {
        SetSceneStatus();
    }
    
    public void SetSceneStatus() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            if (ChangeBgm && _levelConfig.BgmId != BgmId) _levelConfig.SetBgm(BgmId);

            if (ChangeBgp && _levelConfig.BgpId != BgpId) _levelConfig.SetBgp(BgpId);

            if (Math.Abs(WaterHeight - (-64f)) > 0.2f) {
                _levelConfig.SetWaterHeight(WaterHeight);
            }
            
            if (ChangeWeather) {
                _levelConfig.RainyLevel = RainyLevel;
                _levelConfig.FallingStarsLevel = FallingStarsLevel;
                _levelConfig.SnowyLevel = SnowyLevel;
                _levelConfig.ThunderLevel = ThunderLevel;
                _levelConfig.WindyLevel = WindyLevel;
                _levelConfig.Darkness = Darkness;
                _levelConfig.Brightness = Brightness;
            }
        }
    }
    public void LinkWithViewControl() {
        var viewControls = GetTree().GetNodesInGroup("view_control");
        foreach (var node in viewControls) {
            if (node is not ViewControl viewControl) continue;
            var rect = viewControl.ViewRect;
            if (Position.X > rect.Position.X
                && Position.Y > rect.Position.Y
                && Position.X < rect.End.X
                && Position.Y < rect.End.Y) {
                CollisionLayer = 0;
                viewControl.SetSceneControl(this);
            }
        }
    }
    public void LinkWithKoopa() {
        // Todo: LinkedWithKoopa
    }
}
