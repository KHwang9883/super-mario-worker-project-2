using Godot;
using System;

public partial class Mario : CharacterBody2D {
    [Export] private Area2D _waterDetect = null!;

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
    
    public bool InWater;
    public bool OnWaterSurface = true;
    private Area2D _waterArea;

    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    private bool _fire;
    private bool _jump;
    private bool _jumped;
    private int _jumpBoostTimer;
    //private bool _doubleFrameProcess;
    //private bool _lastJumpBoostState;
    public override void _Ready() {
        Area2D waterArea = GetTree().GetFirstNodeInGroup("water") as Area2D;
        _waterArea = waterArea;
        
        if (waterArea == null) {
            GD.PrintErr("Water Not Found.");
            return;
        }
        
        waterArea.BodyEntered += OnWaterBodyEntered;
        waterArea.BodyExited += OnWaterBodyExited;
        
        _waterDetect.AreaEntered += OnWaterBodyEntered;
        _waterDetect.AreaExited += OnWaterBodyExited;
    }

    private void OnWaterBodyEntered(Node2D body) {
        if (body == this) {
            InWater = true;
            _speedY = 0f;
        }
        if (body == _waterArea) {
            OnWaterSurface = false;
        }
    }

    private void OnWaterBodyExited(Node2D body) {
        if (body == this) {
            InWater = false;
        }
        if (body == _waterArea) {
            OnWaterSurface = true;
        }
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
        float acceleration = InWater 
            ? _waterHorizontalAcceleration
            : (_fire ? _runningAcceleration : _walkingAcceleration);

        float maxSpeed;
        
        if (InWater) {
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
            _speedX /= InWater ? 1.03f : 1.05f;
        }
        
        if (_speedX > -0.04f && _speedX < 0.04f) {
            _speedX = 0f;
        }
            
        // y 速度
        // 起跳
        
        if (!_jump) { _jumped = false; }
        
        if (_jump) {
            if (!InWater && IsOnFloor()) {
                _speedY = -(8f + Mathf.Abs(_speedX) / 5f);
                _jumped = true;
            } else if (InWater && !_jumped) {
                _speedY = OnWaterSurface
                    ? -(6f + Mathf.Abs(_speedX) / 5f)
                    : -(4f + Mathf.Abs(_speedX) / 10f);
                _jumped = true;
            }
        }
        
        // 空中跳跃按跳跃键有速度加成
        _jumpBoostTimer = Math.Clamp(_jumpBoostTimer + 1, 0, 2);

        if (_jump && _speedY < 0f && !InWater) {
            if (_jumpBoostTimer > 1) {
                _speedY -= 1.5f;
                _jumpBoostTimer = 0;
            }
        }
        
        // 最大下落速度
        if (!IsOnFloor()) {
            float maxFallSpeed = InWater ? _waterMaxFallingSpeed : _maxFallingSpeed;
    
            if (_speedY > maxFallSpeed) {
                _speedY = maxFallSpeed;
            }
        }

        // 根据GM8版执行顺序在这里进行 MoveAndSlide()
        Velocity = new Vector2(_speedX * FRAMERATE_ORIGIN, (_speedY + _gravity) * FRAMERATE_ORIGIN);
        
        MoveAndSlide();
        
        // 重力
        _speedY += (InWater ? 0.2f : 1.0f);
        
        // 落地或顶头
        if ((IsOnFloor() || (IsOnCeiling() && _speedY < 0f))) {
            _speedY = 0f;
            /*if (InWater) {
                _jumped = true;
            }*/
        }
        
        // GM8版注释：尝试性修复非整格实心穿墙
        // 为保持精确性，故各自方向速度为零分别进行一次取整
        if (_speedX == 0f) {
            Position = new Vector2(Mathf.Round(Position.X), Position.Y);
        }
        if (_speedY == 0f) {
            Position = new Vector2(Position.X, Mathf.Round(Position.Y));
        }
    }
}
