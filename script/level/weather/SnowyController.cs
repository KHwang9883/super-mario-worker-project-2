using Godot;
using System;
using SMWP.Level;

public partial class SnowyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public int SnowyLevel;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }

}
