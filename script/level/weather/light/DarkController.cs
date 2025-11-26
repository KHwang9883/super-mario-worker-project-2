using Godot;
using System;
using SMWP.Level;

public partial class DarkController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public float DarkLevel;
    
    private LevelConfig? _levelConfig;
    private CanvasModulate? _canvasModulate;
    private float _thunderDark;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _canvasModulate = (CanvasModulate)GetTree().GetFirstNodeInGroup("canvas_modulate");
    }
    public override void _Process(double delta) {
        if (_levelConfig == null || _canvasModulate == null) return;
        
        DarkLevel = _levelConfig.Darkness;
        var darkValue = 1f - Mathf.Min(DarkLevel / 9f, _thunderDark);
        _canvasModulate.Color = new Color(darkValue, darkValue, darkValue, 1);
        _thunderDark = Mathf.MoveToward(_thunderDark, DarkLevel / 9f, 0.06f);
    }
    public void OnThunderAppear() {
        _thunderDark = 0f;
    }
}
