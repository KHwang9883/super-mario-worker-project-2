using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class SnowyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] private PackedScene _snowScene = GD.Load<PackedScene>("uid://bo53785p08smn");
    public int SnowyLevel;

    //public static ObjectPool RaindropPool = new();
    private LevelConfig? _levelConfig;
    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        SnowyLevel = _levelConfig.SnowyLevel;
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) return;

        SnowyLevel = _levelConfig.SnowyLevel;
        switch (SnowyLevel) {
            case 0:
                break;
            case 1:
                GetNode<SmwpFrameTimer>("Level1Timer").Start();
                break;
            case 2:
                GetNode<SmwpFrameTimer>("Level2Timer").Start();
                break;
            case 3:
                Create();
                break;
            case 4:
                for (var i = 0; i < 6; i++) {
                    Create();
                }
                break;
            case 5:
                for (var i = 0; i < 16; i++) {
                    Create();
                }
                break;
        }
        GD.Print("SnowyLevel: " + SnowyLevel);
        GD.Print($"Snow Count: {GetTree().GetNodeCountInGroup("snow_pool")}");
    }
    public void Create() {
        var snow = (GetTree().GetNodesInGroup("snow_pool").Count < 10)
            ? _snowScene.Instantiate<Node2D>()
            : (Node2D)GetTree().GetFirstNodeInGroup("snow_pool");
        snow.Position =
            ScreenUtils.GetScreenRect(this).Position +
            new Vector2(-32f + _rng.RandfRange(0f, 960f), -32f);
        snow.ResetPhysicsInterpolation();
        if (snow.IsInGroup("snow_pool")) {
            snow.GetNode<SnowMovement>("SnowMovement").Reset();
            snow.RemoveFromGroup("snow_pool");
        } else {
            _weatherController.AddSibling(snow);
        }
    }
}
