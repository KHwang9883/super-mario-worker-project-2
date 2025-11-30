using Godot;
using System;
using SMWP.Level;

public partial class PlatformVertical : AnimatableBody2D {
    [Export] public float SpeedY = 1f;
    private float _topLimit = 114514f; // Todo: 参数要改
    private float _bottomLimit = -114514f; // Todo: 参数要改
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _bottomLimit += _levelConfig.RoomHeight;
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            // Todo: 桥の上限と下限をLevelConfigから取得するようにする (划)
            if (Position.Y < _topLimit) {
                Position = Position with { Y = 114f }; // Todo
                ResetPhysicsInterpolation();
            }
            if (Position.Y > _bottomLimit) {
                Position = Position with { Y = -1f }; // Todo
                ResetPhysicsInterpolation();
            }
        }
    }
}
