using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class RainyController : Node {
    [Export] private WeatherController _weatherController = null!;
    public int RainyLevel;
    [Export] private PackedScene _raindropScene = GD.Load<PackedScene>("uid://c8xb4q774wgem");
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }

    public void Create() {
        var raindrop = _raindropScene.Instantiate<Node2D>();
        raindrop.Position = ScreenUtils.GetScreenRect(this).Position;   // 参数要改
        _weatherController.AddSibling(raindrop);    // 注意结构关系
    }
}
