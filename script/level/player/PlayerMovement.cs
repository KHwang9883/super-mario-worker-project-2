using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;
using SMWP.Level.Debug;

namespace SMWP.Level.Player;

public partial class PlayerMovement : Node {
    [Signal]
    public delegate void JumpStartedEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;

    // 关卡引力
    private float _gravity = 5f / 5f;
    
    // 运动参数先用 GM8 单位（px/f），计算 Velocity 的时候转换为 Godot 单位（px/s）
    private const float FramerateOrigin = 50f;
    private int _direction = 1;
    public float SpeedX;
    private float _walkingAcceleration = 0.1f;
    private float _runningAcceleration = 0.3f;
    private float _maxWalkingSpeed = 3f;
    private float _maxRunningSpeed = 8f;
    
    public float SpeedY;
    private float _maxFallingSpeed = 13f;
    
    private float _waterHorizontalAcceleration = 0.05f;
    private float _waterMaxWalkingSpeed = 1f;
    private float _waterMaxRunningSpeed = 3f;
    private float _waterMaxFallingSpeed = 6f;
    
    public bool IsInWater;
    private bool _wasInWater;
    public bool IsOnWaterSurface = true;

    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    private bool _fire;
    private bool _jump;
    public bool Jumped;
    private int _jumpBoostTimer;

    // TODO: 绿果状态与冰块状态
    private bool _lui;
    private bool _onIce;

    private Array<Node2D> _results = null!;
    private Array<Node2D> _resultsOutWater = null!;

    private CollisionPolygon2D _blocksPhysicsCollisionSmall = null!;
    private ShapeCast2D _areaBodyCollisionSmall = null!;
    private ShapeCast2D _outWaterDetectSmall = null!;
    private CollisionPolygon2D _blocksPhysicsCollisionSuper = null!;
    private ShapeCast2D _areaBodyCollisionSuper = null!;
    private ShapeCast2D _outWaterDetectSuper = null!;
    
    public override void _Ready() {
        _blocksPhysicsCollisionSmall = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSmall");
        _areaBodyCollisionSmall = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSmall");
        _outWaterDetectSmall = _player.GetNode<ShapeCast2D>("OutWaterDetectSmall");
        _blocksPhysicsCollisionSuper = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSuper");
        _areaBodyCollisionSuper = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSuper");
        _outWaterDetectSuper = _player.GetNode<ShapeCast2D>("OutWaterDetectSuper");
    }

    public override void _PhysicsProcess(double delta) {
        // 输入处理
        _up = Input.IsActionPressed("move_up");
        _down = Input.IsActionPressed("move_down");
        _left = Input.IsActionPressed("move_left");
        _right = Input.IsActionPressed("move_right");
        _fire = Input.IsActionPressed("move_fire");
        _jump = Input.IsActionPressed("move_jump");

        // x 速度
        float acceleration = IsInWater
            ? _waterHorizontalAcceleration
            : (_fire ? _runningAcceleration : _walkingAcceleration);

        float maxSpeed;

        if (IsInWater) {
            maxSpeed = _fire ? _waterMaxRunningSpeed : _waterMaxWalkingSpeed;
        } else {
            if (_fire) {
                maxSpeed = _maxRunningSpeed;
            } else {
                maxSpeed = Mathf.Abs(SpeedX) > _maxWalkingSpeed
                    ? _maxRunningSpeed
                    : _maxWalkingSpeed;
            }
        }

        if (_left) {
            SpeedX = Mathf.Clamp(SpeedX - acceleration, -maxSpeed, maxSpeed);
        }
        if (_right) {
            SpeedX = Mathf.Clamp(SpeedX + acceleration, -maxSpeed, maxSpeed);
        }

        if (!_left && !_right) {
            SpeedX /= IsInWater ? 1.03f : 1.05f;
        }

        if (SpeedX > -0.04f && SpeedX < 0.04f) {
            SpeedX = 0f;
        }

        // y 速度
        // 落地或顶头
        if ((_player.IsOnFloor() || (_player.IsOnCeiling() && SpeedY < 0f))) {
            SpeedY = 0f;
        }

        // 起跳

        if (!_jump) {
            Jumped = false;
        }

        if (_jump) {
            if (!IsInWater && _player.IsOnFloor()) {
                SpeedY = -(8f + Mathf.Abs(SpeedX) / 5f);
                Jumped = true;
                EmitSignal(SignalName.JumpStarted);
            } else if (IsInWater && !Jumped) {
                SpeedY = IsOnWaterSurface
                    ? -(6f + Mathf.Abs(SpeedX) / 5f)
                    : -(4f + Mathf.Abs(SpeedX) / 10f);
                Jumped = true;
                EmitSignal(SignalName.JumpStarted);
            }
        }

        // 空中跳跃按跳跃键有速度加成
        _jumpBoostTimer = Math.Clamp(_jumpBoostTimer + 1, 0, 2);

        if (_jump && SpeedY < 0f && !IsInWater) {
            if (_jumpBoostTimer > 1) {
                SpeedY -= 1.5f;
                _jumpBoostTimer = 0;
            }
        }

        // 最大下落速度
        if (!_player.IsOnFloor()) {
            float maxFallSpeed = IsInWater ? _waterMaxFallingSpeed : _maxFallingSpeed;

            if (SpeedY > maxFallSpeed) {
                SpeedY = maxFallSpeed;
            }
        }

        // 根据GM8版执行顺序在这里进行速度计算并 MoveAndSlide()
        _player.Velocity = new Vector2(SpeedX * FramerateOrigin, (SpeedY + _gravity) * FramerateOrigin);

        _player.MoveAndSlide();

        // 重叠物件检测
        try {
            // TODO: 替换硬编码为NodePath以支持不同状态下的碰撞箱
            _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>("AreaBodyCollisionSmall"));
            _resultsOutWater =
                ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>("OutWaterDetectSmall"));
        } catch {
            GD.PrintErr("重叠物件检测失败");
            GD.Print("启用临时重力修正");
            SpeedY += (IsInWater ? 0.2f : 1.0f);
        }

        // 水中状态检测
        IsInWater = false;
        foreach (var result in _results) {
            if (result.IsInGroup("water")) {
                IsInWater = true;
            }
        }
        if (_wasInWater != IsInWater) {
            SpeedY = Mathf.Min(0f, SpeedY);
        }
        _wasInWater = IsInWater;

        IsOnWaterSurface = true;
        foreach (var resultOutWater in _resultsOutWater) {
            if (resultOutWater.IsInGroup("water")) {
                IsOnWaterSurface = false;
            }
        }

        // Debug
        //GD.Print(ShapeQueryResult.ShapeQuery(this, GetNode<ShapeCast2D>("AreaBodyCollision")));

        // 重力
        SpeedY += (IsInWater ? 0.2f : 1.0f);

        // GM8版注释：尝试性修复非整格实心穿墙
        // 为保持精确性，故各自方向速度为零分别进行一次取整，但是该做法会导致楼梯地形贴墙原地起跳边缘卡脚，因此禁用

        // 不同状态的碰撞箱切换
        Callable.From(() => {
            if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Small) {
                _blocksPhysicsCollisionSmall.Disabled = false;
                _areaBodyCollisionSmall.Enabled = true;
                _outWaterDetectSmall.Enabled = true;
                _blocksPhysicsCollisionSuper.Disabled = true;
                _areaBodyCollisionSuper.Enabled = false;
                _outWaterDetectSuper.Enabled = false;
            } else {
                _blocksPhysicsCollisionSmall.Disabled = true;
                _areaBodyCollisionSmall.Enabled = false;
                _outWaterDetectSmall.Enabled = false;
                _blocksPhysicsCollisionSuper.Disabled = false;
                _areaBodyCollisionSuper.Enabled = true;
                _outWaterDetectSuper.Enabled = true;
            }
        }).CallDeferred();

        // For debug use
        _areaBodyCollisionSmall.Visible = _areaBodyCollisionSmall.Enabled;
        _areaBodyCollisionSuper.Visible = _areaBodyCollisionSuper.Enabled;
    }

    public Array<Node2D> GetShapeQueryResults() {
        return _results;
    }
}
