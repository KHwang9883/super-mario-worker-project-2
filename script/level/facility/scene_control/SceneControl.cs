using Godot;
using System;
using SMWP.Level;

public partial class SceneControl : Area2D {
    [Export] public bool ChangeBgm;
    [Export] public int BgmId = 1;
    [Export] public bool ChangeBgp;
    [Export] public int BgpId = 1;
    public bool LinkedWithViewControl;
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
    }
    public void OnBodyEntered(Node2D body) {
        SetSceneStatus();
    }
    
    // Todo: if (LinkedWithViewControl)
    
    public void SetSceneStatus() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: Level Config is null!");
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
}
