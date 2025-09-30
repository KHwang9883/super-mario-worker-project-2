using Godot;
using System;

public partial class Mario : CharacterBody2D
{
    private float _speed = 0.0f;
    private float _walkingAcceleration = 0.1f * 50.0f;
    private float _runningAcceleration = 0.3f * 50.0f;
    private float _maxWalkingSpeed = 3.0f * 50.0f;
    private float _maxRunningSpeed = 8.0f * 50.0f;
    private int _direction = 1;

    public float Gravity = 5.0f / 5.0f * 50.0f;
    private float _maxFallingSpeed = 13.0f * 50.0f;

    private bool _doubleFrameProcess = false;
    //private float jumpBoostTimer = 0.0f;

    private bool _inWater = false;

    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    private bool _fire;
    private bool _jump;

    public override void _PhysicsProcess(double delta) {
        // 输入处理
        _up = Input.IsActionPressed("move_up");
        _down = Input.IsActionPressed("move_down");
        _left = Input.IsActionPressed("move_left");
        _right = Input.IsActionPressed("move_right");
        _fire = Input.IsActionPressed("move_fire");
        _jump = Input.IsActionPressed("move_jump");
            
        // x 速度
        if (!_fire) {
            if (_left) {
                _speed = Mathf.Clamp(_speed - _walkingAcceleration, -_maxWalkingSpeed, _maxWalkingSpeed);
            }
            if (_right) {
                _speed = Mathf.Clamp(_speed + _walkingAcceleration, -_maxWalkingSpeed, _maxWalkingSpeed);
            }
        } else {
            if (_left) {
                _speed = Mathf.Clamp(_speed - _runningAcceleration, -_maxRunningSpeed, _maxRunningSpeed);
            }
            if (_right) {
                _speed = Mathf.Clamp(_speed + _runningAcceleration, -_maxRunningSpeed, _maxRunningSpeed);
            }
        }
        if (!_left && !_right) {
            _speed /= 1.05f;
        }
        if (_speed > -0.04f && _speed < 0.04f) {
            _speed = 0.0f;
        }
        
        Velocity = new Vector2(_speed, Velocity.Y);
            
        // y 速度
        if (IsOnFloor()) {
            _doubleFrameProcess = true;
        }
        
        if (IsOnFloor() && _jump) {
            Velocity = new Vector2(Velocity.X, -(8.0f + Mathf.Abs(_speed / 50.0f) / 5.0f) * 50.0f);
        }
            
        if (_jump && Velocity.Y < 0 && !_inWater && _doubleFrameProcess) {
            Velocity = new Vector2(Velocity.X, Velocity.Y - 1.5f * 50.0f);
        }
        
        _doubleFrameProcess = !_doubleFrameProcess;
        
        if (!_inWater) {
            Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity);
        }
            
        if (!IsOnFloor() && Velocity.Y > _maxFallingSpeed) {
            Velocity = new Vector2(Velocity.X, _maxFallingSpeed);
        }

        if (IsOnCeiling() && Velocity.Y < 0) {
            Velocity = new Vector2(Velocity.X, 0);
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