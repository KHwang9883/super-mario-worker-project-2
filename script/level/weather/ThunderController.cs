using Godot;
using System;
using SMWP.Level;

public partial class ThunderController : Node {
    [Export] private WeatherController _weatherController = null!;
    public int ThunderLevel;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }

}
