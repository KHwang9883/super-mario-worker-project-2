using Godot;
using System;
using SMWP.Level;

public partial class WindyController : Node {
    [Export] private WeatherController _weatherController = null!;
    public int WindyLevel;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }

}
