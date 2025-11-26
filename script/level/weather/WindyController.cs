using Godot;
using System;
using SMWP.Level;

public partial class WindyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public int WindyLevel;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) return;

        WindyLevel = _levelConfig.WindyLevel;
    }
}
