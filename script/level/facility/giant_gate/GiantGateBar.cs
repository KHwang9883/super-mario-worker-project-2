using Godot;
using System;
using System.Diagnostics;
using SMWP;
using SMWP.Level;
using SMWP.Level.Score;

public partial class GiantGateBar : Area2D {
    [Signal]
    public delegate void PlaySoundLevelPassEventHandler();
    [Signal]
    public delegate void PlaySoundFasterLevelPassEventHandler();
    
    [Export] private Sprite2D? _barOn;
    [Export] private Sprite2D? _barOff;
    [Export] private AddScoreComponent? _addScoreComponent;

    [Export] private PackedScene _smokeScene = GD.Load<PackedScene>("uid://c707h2fhiiirw");
    
    public enum GateBarStateEnum { Moving, Triggered }
    private GateBarStateEnum _gateBarState = GateBarStateEnum.Moving;
    private int _sequence;
    private float _yRecord;

    private float _speedX = -5f;
    private float _speedY = -4f;

    public override void _PhysicsProcess(double delta) {
        switch (_gateBarState) {
            case GateBarStateEnum.Moving:
                if (_sequence == 0) {
                    if (_yRecord < 200) {
                        Position += new Vector2(0, 3);
                        _yRecord += 3;
                    } else _sequence = 1;
                } else if (_yRecord > 0) {
                    Position -= new Vector2(0, 3);
                    _yRecord -= 3;
                } else _sequence = 0;
                break;
            case GateBarStateEnum.Triggered:
                if (_barOn != null) _barOn.Visible = false;
                if (_barOff != null) _barOff.Visible = true;
                Position += new Vector2(_speedX, _speedY);
                _speedY += 0.2f;
                RotationDegrees += 10f;
                break;
        }
    }
    public void OnBodyEntered(Node2D body) {
        if (!body.IsInGroup("player")) return;
        _gateBarState = GateBarStateEnum.Triggered;
        ProcessMode = ProcessModeEnum.Always;
        
        // 原版中原意可能是根据玩家碰撞位置决定运动方向，但是没有实现
        /*if (Position.X < body.Position.X) {
            _speedX = 5f;
        } else {
            _speedX = -5f;
        }*/
        
        // 分数
        if (_addScoreComponent != null) {
            _addScoreComponent.InternalScore = _yRecord switch {
                >= 0 and < 10 => 10000,
                >= 10 and < 30 => 5000,
                >= 30 and < 50 => 2000,
                >= 50 and < 70 => 1000,
                >= 70 and < 100 => 500,
                >= 100 and < 140 => 200,
                >= 140 and <= 200 => 100,
                _ => _addScoreComponent.InternalScore,
            };
            _addScoreComponent.AddScore();
        }

        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        // 音效
        EmitSignal(levelConfig.FasterLevelPass
            ? SignalName.PlaySoundFasterLevelPass
            : SignalName.PlaySoundLevelPass);

        // Smoke Effect
        if (levelConfig.FasterLevelPass) {
            var smoke = _smokeScene.Instantiate<Node2D>();
            smoke.Position = Position;
            AddSibling(smoke);
        }
        
        GameManager.IsLevelPass = true;
        GetTree().Paused = true;
    }
}
