using Godot;
using System;
using SMWP.Level;

public partial class LevelCamera : Camera2D {
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        LimitRight = (int)_levelConfig.RoomWidth;
        LimitBottom = (int)_levelConfig.RoomHeight;
    }
}
