using Godot;
using System;
using SMWP.Level;

public partial class BrightController : Node {
    [Export] private WeatherController _weatherController = null!;
    public int BrightLevel;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }
    public override void _Process(double delta) {
        if (_levelConfig == null) return;
        
        BrightLevel = _levelConfig.Brightness;
    }
}
