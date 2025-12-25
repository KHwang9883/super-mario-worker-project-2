using Godot;
using System;
using SMWP.Level;

public partial class SwitchSound : AudioStreamPlayer2D {
    private LevelConfig? _levelConfig;
    private float _originVolumeDb;
    
    public override void _Ready() {
        base._Ready();
        _originVolumeDb = VolumeDb;
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (_levelConfig == null) {
            GD.PushError($"{this.Name}: LevelConfig is null!");
            return;
        }
        if (Playing && !_levelConfig.SwitchSound) {
            Stop();
        }
    }
}
