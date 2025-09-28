using Godot;
using System;

public partial class Mario : CharacterBody2D
{
    private float speed = 0.0f;
    private readonly float walkingAcceleration = 0.1f * 50.0f;
    private readonly float runningAcceleration = 0.3f * 50.0f;
    private readonly float maxWalkingSpeed = 3.0f * 50.0f;
    private readonly float maxRunningSpeed = 8.0f * 50.0f;
    private int direction = 1;

    private readonly float gravity = 1.0f * 50.0f;
    private readonly float maxFallingSpeed = 13.0f * 50.0f;
    private float jumpBoostTimer = 0.0f;

    private bool inWater = false;

    private bool up;
    private bool down;
    private bool left;
    private bool right;
    private bool fire;
    private bool jump;

    public override void _PhysicsProcess(double delta)
    {
        // 输入处理
        up = Input.IsActionPressed("move_up");
        down = Input.IsActionPressed("move_down");
        left = Input.IsActionPressed("move_left");
        right = Input.IsActionPressed("move_right");
        fire = Input.IsActionPressed("move_fire");
        jump = Input.IsActionPressed("move_jump");
            
        // x 速度
        if (!fire)
        {
            if (left)
            {
                speed = Mathf.Clamp(speed - walkingAcceleration, -maxWalkingSpeed, maxWalkingSpeed);
            }
            if (right)
            {
                speed = Mathf.Clamp(speed + walkingAcceleration, -maxWalkingSpeed, maxWalkingSpeed);
            }
        }
        else
        {
            if (left)
            {
                speed = Mathf.Clamp(speed - runningAcceleration, -maxRunningSpeed, maxRunningSpeed);
            }
            if (right)
            {
                speed = Mathf.Clamp(speed + runningAcceleration, -maxRunningSpeed, maxRunningSpeed);
            }
        }
            
        if (!left && !right)
        {
            speed /= 1.05f;
        }
            
        if (speed > -0.04f && speed < 0.04f)
        {
            speed = 0.0f;
        }
            
        Velocity = new Vector2(speed, Velocity.Y);
            
        // y 速度
        if (IsOnFloor() && jump)
        {
            Velocity = new Vector2(Velocity.X, -(8.0f + Mathf.Abs(speed / 50.0f) / 5.0f) * 50.0f);
        }
            
        if (jump && Velocity.Y < 0 && !inWater)
        {
            Velocity = new Vector2(Velocity.X, Velocity.Y - 0.78f * 50.0f);
        }
            
        if (!inWater)
        {
            Velocity = new Vector2(Velocity.X, Velocity.Y + gravity);
        }
            
        if (!IsOnFloor() && Velocity.Y > maxFallingSpeed)
        {
            Velocity = new Vector2(Velocity.X, maxFallingSpeed);
        }
            
        MoveAndSlide();
    }
}