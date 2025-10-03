using Godot;
using System;

public partial class Mario : CharacterBody2D {
    [Export] private Area2D _waterDetect = null!;

    public float Gravity = 5.0f / 5.0f;
    
    private int _direction = 1;
    private float _speedX;
    private float _walkingAcceleration = 0.1f * 2500.0f;
    private float _runningAcceleration = 0.3f * 2500.0f;
    private float _maxWalkingSpeed = 3.0f * 50.0f;
    private float _maxRunningSpeed = 8.0f * 50.0f;
    
    //private float _speedY;
    private float _maxFallingSpeed = 13.0f * 50.0f;

    private Resource _waterBoundary = null!;
    
    private float _waterHorizontalAcceleration = 0.05f * 2500.0f;
    private float _waterMaxWalkingSpeed = 1.0f * 50.0f;
    private float _waterMaxRunningSpeed = 3.0f * 50.0f;
    private float _waterMaxFallingSpeed = 6.0f * 50.0f;

    private bool _doubleFrameProcess;
    //private float jumpBoostTimer = 0.0f;

    public bool InWater;
    public bool OnWaterSurface;

    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    private bool _fire;
    private bool _jump;
    private bool _jumped;

    public override void _Ready() {
        Area2D waterArea = GetTree().GetFirstNodeInGroup("water") as Area2D;
        
        if (waterArea == null) {
            GD.PrintErr("Water Not Found.");
            return;
        }
        
        waterArea.BodyEntered += OnWaterBodyEntered;
        waterArea.BodyExited += OnWaterBodyExited;
    }

    private void OnWaterBodyEntered(Node2D body) {
        if (body == this) {
            InWater = true;
            Velocity = new Vector2(Velocity.X, 0.0f);
        }
        if (body == _waterDetect) {
            OnWaterSurface = false;
        }
    }

    private void OnWaterBodyExited(Node2D body) {
        if (body == this) {
            InWater = false;
        }
        if (body == _waterDetect) {
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

        float maxSpeed = InWater 
            ? (_fire ? _waterMaxRunningSpeed : _waterMaxWalkingSpeed)
            : (_fire ? _maxRunningSpeed : _maxWalkingSpeed);

        if (_left) {
            _speedX = Mathf.Clamp(_speedX - acceleration * (float)delta, -maxSpeed, maxSpeed);
        }
        if (_right) {
            _speedX = Mathf.Clamp(_speedX + acceleration * (float)delta, -maxSpeed, maxSpeed);
        }

        if (!_left && !_right) {
            _speedX /= InWater ? 1.03f : 1.05f;
        }
        
        if (_speedX > -0.04f && _speedX < 0.04f) {
            _speedX = 0.0f;
        }
        
        Velocity = new Vector2(_speedX, Velocity.Y);
            
        // y 速度
        // 起跳
        if (IsOnFloor()) {
            _doubleFrameProcess = true;
        }
        
        if (!_jump) { _jumped = false; }
        
        if (_jump) {
            if (!InWater && IsOnFloor()) {
                Velocity = new Vector2(Velocity.X, -(8.0f + Mathf.Abs(_speedX / 50.0f) / 5.0f) * 50.0f);
            } else if (InWater && !_jumped) {
                Velocity = new Vector2(
                    Velocity.X,
                    OnWaterSurface
                        ? -(6.0f + Mathf.Abs(_speedX / 50.0f) / 5.0f) * 50.0f
                        : -(4.0f + Mathf.Abs(_speedX / 50.0f) / 10.0f) * 50.0f
                );
                _jumped = true;
            }
        }
        
        // 空中跳跃按跳跃键有速度加成
        if (_jump && Velocity.Y < 0 && !InWater && _doubleFrameProcess) {
            Velocity = new Vector2(Velocity.X, Velocity.Y - 1.5f * 2500.0f * (float)delta);
        }
        
        _doubleFrameProcess = !_doubleFrameProcess;
        
        // 重力
        Velocity = new Vector2(Velocity.X,
            Velocity.Y + (InWater ? 0.2f : 1.0f) * 2500.0f * (float)delta + Gravity
        );
        
        // 最大下落速度
        if (!IsOnFloor()) {
            float maxFallSpeed = InWater ? _waterMaxFallingSpeed : _maxFallingSpeed;
    
            if (Velocity.Y > maxFallSpeed) {
                Velocity = new Vector2(Velocity.X, maxFallSpeed);
            }
        }
        
        // 顶头
        if (IsOnCeiling() && Velocity.Y < 0) {
            Velocity = new Vector2(Velocity.X, 0.0f);
        }
        
        MoveAndSlide();
        
        // GM8版注释：尝试性修复非整格实心穿墙
        // 为保持精确性，故撞墙、撞天花板和落地进行一次取整
        if (IsOnWall()) {
            Position = new Vector2(Mathf.Round(Position.X), Position.Y);
        }
        if (IsOnFloor() || IsOnCeiling()) {
            Position = new Vector2(Position.X, Mathf.Round(Position.Y));
        }
    }
}
