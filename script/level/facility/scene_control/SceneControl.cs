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
    private int _activateTimer;
    private bool _triggerred;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);

        Callable.From(SetLinkedObject).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (_activateTimer < 2) {
            _activateTimer++;
            return;
        }
        if (_triggerred) {
            return;
        }
        var bodies = GetOverlappingBodies();
        if (bodies.Count > 0) {
            SetSceneStatus();
        }
    }
    
    public void OnBodyEntered(Node2D body) {
        if (_activateTimer < 2) {
            return;
        }
        SetSceneStatus();
    }
    public void OnBodyExited(Node2D body) {
        _triggerred = false;
    }

    public void SetLinkedObject() {
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
    
    public void SetSceneStatus() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            if (ChangeBgm && _levelConfig.BgmId != BgmId) _levelConfig.SetBgm(BgmId);

            if (ChangeBgp && _levelConfig.BgpId != BgpId) _levelConfig.SetBgp(BgpId);

            if (WaterHeight > -64f) {
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
                && Position.X < rect.Position.X + 64f
                && Position.Y < rect.Position.Y + 32f) {
                CollisionLayer = 0;
                viewControl.SetSceneControl(this);
            }
        }
    }
    public void LinkWithKoopa() {
        var koopaScrolls = GetTree().GetNodesInGroup("koopa_scroll");
        foreach (var node in koopaScrolls) {
            if (node is not KoopaScroll koopaScroll) continue;
            var rect = new Rect2(
                koopaScroll.SceneMarkerPos.GlobalPosition, 
                koopaScroll.SceneMarkerSize.GlobalPosition
                );
            if (Position.X > rect.Position.X
                && Position.Y > rect.Position.Y
                && Position.X < rect.End.X
                && Position.Y < rect.End.Y) {
                CollisionLayer = 0;
                koopaScroll.SetSceneControl(this);
            }
        }
    }
}
