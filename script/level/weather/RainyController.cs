using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class RainyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] private PackedScene _raindropScene = GD.Load<PackedScene>("uid://c8xb4q774wgem");
    [Export] public int RainyLevel;

    //public static ObjectPool RaindropPool = new();
    private LevelConfig? _levelConfig;
    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        RainyLevel = _levelConfig.RainyLevel;
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) return;

        RainyLevel = _levelConfig.RainyLevel;
        switch (RainyLevel) {
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
        //GD.Print("RainyLevel: " + RainyLevel);
        GD.Print($"Raindrops Count: {GetParent().GetParent().GetChildCount()}");
    }
    public void Create() {
        var raindrop = (GetTree().GetNodesInGroup("raindrop_pool").Count < 60)
            ? _raindropScene.Instantiate<Node2D>()
            : (Node2D)GetTree().GetFirstNodeInGroup("raindrop_pool");
        raindrop.Position =
            ScreenUtils.GetScreenRect(this).Position +
            new Vector2(-32f + _rng.RandfRange(0f, 960f), -32f);
        if (raindrop.IsInGroup("raindrop_pool")) {
            raindrop.GetNode<RaindropMovement>("RaindropMovement").Reset();
            raindrop.Visible = true;
            raindrop.RemoveFromGroup("raindrop_pool");
        } else {
            _weatherController.AddSibling(raindrop);
        }
    }
}
