using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;
using SMWP.Level.Debug;

namespace SMWP.Level.Player;

public partial class PlayerMovement : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;

    // 关卡引力
    private float _gravity = 5f / 5f;
    
    // 运动参数先用 GM8 单位（px/f），计算 Velocity 的时候转换为 Godot 单位（px/s）
    private const float FRAMERATE_ORIGIN = 50f;
    private int _direction = 1;
    private float _speedX;
    private float _walkingAcceleration = 0.1f;
    private float _runningAcceleration = 0.3f;
    private float _maxWalkingSpeed = 3f;
    private float _maxRunningSpeed = 8f;
    
    private float _speedY;
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
    private bool _jumped;
    private int _jumpBoostTimer;

    private Array<Node2D> _results = null!;
    
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
                maxSpeed = Mathf.Abs(_speedX) > _maxWalkingSpeed 
                    ? _maxRunningSpeed 
                    : _maxWalkingSpeed;
            }
        }

        if (_left) {
            _speedX = Mathf.Clamp(_speedX - acceleration, -maxSpeed, maxSpeed);
        }
        if (_right) {
            _speedX = Mathf.Clamp(_speedX + acceleration, -maxSpeed, maxSpeed);
        }

        if (!_left && !_right) {
            _speedX /= IsInWater ? 1.03f : 1.05f;
        }
        
        if (_speedX > -0.04f && _speedX < 0.04f) {
            _speedX = 0f;
        }
            
        // y 速度
        // 落地或顶头
        if ((_player.IsOnFloor() || (_player.IsOnCeiling() && _speedY < 0f))) {
            _speedY = 0f;
        }
        
        // 起跳
        
        if (!_jump) { _jumped = false; }
        
        if (_jump) {
            if (!IsInWater && _player.IsOnFloor()) {
                _speedY = -(8f + Mathf.Abs(_speedX) / 5f);
                _jumped = true;
            } else if (IsInWater && !_jumped) {
                _speedY = IsOnWaterSurface
                    ? -(6f + Mathf.Abs(_speedX) / 5f)
                    : -(4f + Mathf.Abs(_speedX) / 10f);
                _jumped = true;
            }
        }
        
        // 空中跳跃按跳跃键有速度加成
        _jumpBoostTimer = Math.Clamp(_jumpBoostTimer + 1, 0, 2);

        if (_jump && _speedY < 0f && !IsInWater) {
            if (_jumpBoostTimer > 1) {
                _speedY -= 1.5f;
                _jumpBoostTimer = 0;
            }
        }
        
        // 最大下落速度
        if (!_player.IsOnFloor()) {
            float maxFallSpeed = IsInWater ? _waterMaxFallingSpeed : _maxFallingSpeed;
    
            if (_speedY > maxFallSpeed) {
                _speedY = maxFallSpeed;
            }
        }

        // 根据GM8版执行顺序在这里进行速度计算并 MoveAndSlide()
        _player.Velocity = new Vector2(_speedX * FRAMERATE_ORIGIN, (_speedY + _gravity) * FRAMERATE_ORIGIN);
        
        _player.MoveAndSlide();
        
        // 重叠物件检测
        _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>("AreaBodyCollision"));
        var resultsOutWater = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>("OutWaterDetect"));
        
        // 水中状态检测
        IsInWater = false;
        foreach (var result in _results) {
            if (result.IsInGroup("water")) {
                IsInWater = true;
            }
        }
        if (_wasInWater != IsInWater) {
            _speedY = Mathf.Min(0f, _speedY);
        }
        _wasInWater = IsInWater;

        IsOnWaterSurface = true;
        foreach (var resultOutWater in resultsOutWater) {
            if (resultOutWater.IsInGroup("water")) {
                IsOnWaterSurface = false;
            }
        }
        
        // Debug
        //GD.Print(ShapeQueryResult.ShapeQuery(this, GetNode<ShapeCast2D>("AreaBodyCollision")));
        
        // 重力
        _speedY += (IsInWater ? 0.2f : 1.0f);
        
        // GM8版注释：尝试性修复非整格实心穿墙
        // 为保持精确性，故各自方向速度为零分别进行一次取整，但是该做法会导致楼梯地形贴墙原地起跳边缘卡脚，因此禁用
        /*if (_speedX == 0f) {
            _player.Position = new Vector2(Mathf.Round(_player.Position.X), _player.Position.Y);
        }
        if (_speedY == 0f) {
            _player.Position = new Vector2(_player.Position.X, Mathf.Round(_player.Position.Y));
        }*/
    }

    public Array<Node2D> GetShapeQueryResults() {
        return _results;
    }
}
