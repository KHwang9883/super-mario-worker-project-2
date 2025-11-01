using Godot;
using System;

public partial class BasicMovement : Node {
    [Export] private CharacterBody2D _moveObject = null!;
    [Export] private float _speedX = -1f;
    [Export] private float _gravity = 0.5f;
    [Export] private bool _edgeDetect;
    private const float FramerateOrigin = 50f;
    private float _speedY;
    
    public override void _PhysicsProcess(double delta) {
        // x 速度
        if (_moveObject.IsOnWall()) {
            _speedX *= -1f;
        }
        
        // y 速度
        if (!_moveObject.IsOnFloor()) {
            _speedY += _gravity;
        } else {
            _speedY = 0f;
        }
        
        _moveObject.Velocity = new Vector2(_speedX * FramerateOrigin, _speedY * FramerateOrigin);
        
        _moveObject.MoveAndSlide();
    }
}
