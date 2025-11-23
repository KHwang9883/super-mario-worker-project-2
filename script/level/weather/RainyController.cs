using Godot;
using System;
using SMWP.Level.Tool;

public partial class RainyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 5, 1")] public int RainyLevel;
    [Export] private PackedScene _raindropScene = GD.Load<PackedScene>("uid://c8xb4q774wgem");

    public void Create() {
        var raindrop = _raindropScene.Instantiate<Node2D>();
        raindrop.Position = ScreenUtils.GetScreenRect(this).Position;   // 参数要改
        _weatherController.AddSibling(raindrop);    // 注意结构关系
    }
}
