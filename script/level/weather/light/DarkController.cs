using Godot;
using System;
using SMWP.Level;

public partial class DarkController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public float DarkLevel;
    
    private LevelConfig? _levelConfig;
    private CanvasModulate? _canvasModulate;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _canvasModulate = (CanvasModulate)GetTree().GetFirstNodeInGroup("canvas_modulate");
    }
    public override void _Process(double delta) {
        if (_levelConfig == null || _canvasModulate == null) return;
        
        DarkLevel = _levelConfig.Darkness;
        _canvasModulate.Color = new Color(
            1f - DarkLevel / 9f,
            1f - DarkLevel / 9f,
            1f - DarkLevel / 9f,
            1
        );
    }
}
