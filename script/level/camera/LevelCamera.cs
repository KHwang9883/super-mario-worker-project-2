using Godot;
using System;
using SMWP.Level;

public partial class LevelCamera : Camera2D {
    private LevelConfig? _levelConfig;
    private CharacterBody2D? _player;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        LimitRight = (int)_levelConfig.RoomWidth;
        LimitBottom = (int)_levelConfig.RoomHeight;
        _player ??= (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
    }

    public override void _PhysicsProcess(double delta) {
        if (_player == null) {
            GD.PushError($"{this}: Player is null!");
        } else {
            // Todo: 修复摄像机在玩家站上运动桥上抖动的问题
            Position = _player.Position.Round();
        }
    }
}
