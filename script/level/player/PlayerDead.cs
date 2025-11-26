using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerDead : Node2D {
    [Signal]
    public delegate void PlaySoundDieEventHandler();
    [Signal]
    public delegate void PlaySoundFastRetryEventHandler();

    private LevelConfig? _levelConfig;
    private float _speedY = -6f;
    private int _timer;

    public override void _Ready() {
        _levelConfig = (LevelConfig)LevelConfigAccess.GetLevelConfig(this);
        EmitSignal(!_levelConfig.FastRetry
            ? SignalName.PlaySoundDie
            : SignalName.PlaySoundFastRetry
            );
        _speedY -= !_levelConfig.FastRetry ? 0.4f : 1.6f;
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }

        var time = !_levelConfig.FastRetry ? 50 : 25;
        if (_timer < time) {
            _timer++;
        } else {
            Position = new Vector2(Position.X, Position.Y + _speedY);
            if (!(_speedY < 10f)) return;
            _speedY += 0.4f * (!_levelConfig.FastRetry ? 1f : 2f);
        }
    }
}
