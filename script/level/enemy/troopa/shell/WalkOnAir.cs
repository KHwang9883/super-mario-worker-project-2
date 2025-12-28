using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Physics;

public partial class WalkOnAir : Node {
    [Export] private BasicMovement? _basicMovement;

    private LevelConfig? _levelConfig;
    private float _lastOnLandPosY;
    private bool _detect;

    public override void _Ready() {
        if (_basicMovement == null) {
            GD.PushError($"WalkOnAir: _basicMovement is null!");
            return;
        }
        _lastOnLandPosY = _basicMovement.MoveObject.Position.Y;
        
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        // SMWP2 关卡不支持旧式悬空龟壳
        Callable.From(() => {
            if (_levelConfig.SmwpVersion >= 2000) {
                QueueFree();
            }
        }).CallDeferred();
    }

    public override void _PhysicsProcess(double delta) {
        if (_basicMovement == null) {
            GD.PushError($"WalkOnAir: _basicMovement is null!");
            return;
        }
        if (!_basicMovement.MoveObject.IsOnFloor()) {
            _detect = true;
        }
        if (!_detect) {
            return;
        }
        if (_basicMovement.MoveObject.IsOnFloor()) {
            if (Math.Abs(_basicMovement.MoveObject.Position.Y - _lastOnLandPosY) is < 12f and > 0.8f) {
                _basicMovement.Gravity = 0f;
            }
            _lastOnLandPosY = _basicMovement.MoveObject.Position.Y;
            _detect = false;
        }
    }
}
