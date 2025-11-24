using Godot;
using System;
using SMWP.Level;

public partial class FallingStarsController : Node {
    [Export] private WeatherController _weatherController = null!;
    public int FallingStarsLevel;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }

}
