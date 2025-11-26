using Godot;
using System;
using SMWP.Level;

public partial class ThunderController : Node {
    [Signal]
    public delegate void ThunderEventHandler();
    
    [Export] private WeatherController _weatherController = null!;
    [Export] private PackedScene _thunderScene = GD.Load<PackedScene>("uid://djcjjv16w874t");
    public int ThunderLevel;
    private int _time;
    private int _timer;
    private RandomNumberGenerator _rng = new();
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _time = 150 + _rng.RandiRange(0, 50);
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) return;
        
        ThunderLevel = _levelConfig.ThunderLevel;
        if (ThunderLevel == 0) return;
        _timer++;
        if (_timer < _time) return;
        _timer = 0;
        _time = 150 + _rng.RandiRange(0, 50);
        if (_rng.RandiRange(0, 100) < 30) return;
        var thunder = _thunderScene.Instantiate<Node2D>();
        GetTree().GetFirstNodeInGroup("thunder_parallax").AddChild(thunder);
        EmitSignal(SignalName.Thunder);
    }
}
