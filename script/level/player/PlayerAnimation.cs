using Godot;
using System;
using SMWP.Level.Player;

public partial class PlayerAnimation : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private CharacterBody2D _player = null!;

    [Export] private SpriteFrames _marioSmall = null!;
    [Export] private SpriteFrames _marioSuper = null!;
    [Export] private SpriteFrames _marioFireball = null!;
    [Export] private SpriteFrames _marioBeetroot = null!;
    [Export] private SpriteFrames _marioLui = null!;
    
    private PlayerMovement _playerMovement;
    private float _imageIndex;

    public override void _Ready() {
        _playerMovement = _playerMediator.GetNode<PlayerMovement>("PlayerMovement");
        _playerMovement.JumpStarted += OnJumpStarted;
    }
    public override void _PhysicsProcess(double delta) {
        // Direction
        if (_playerMovement.SpeedX > 0f) {
            _animatedSprite2D.FlipH = false;
        } else if (_playerMovement.SpeedX < 0f) {
            _animatedSprite2D.FlipH = true;
        }
        
        // Walk Jump Idle
        if (!_playerMovement.IsInWater) {
            if (!_player.IsOnFloor()) {
                _animatedSprite2D.Animation = "jump";
            } else {
                if (Mathf.Abs(_player.Velocity.X) < 0.1f) {
                    _animatedSprite2D.Animation = "idle";
                } else {
                    _animatedSprite2D.Animation = "walk";
                    
                    _imageIndex += _playerMovement.SpeedX / 10f;
                    int frameCount = _animatedSprite2D.SpriteFrames.GetFrameCount("walk");
                    if (frameCount > 0) {
                        int frame = (int)Mathf.Abs(_imageIndex) % frameCount;
                        _animatedSprite2D.Frame = frame;
                    }
                }
            }
        } else {
            if (!_player.IsOnFloor()) {
                _animatedSprite2D.Play("swim");
            } else {
                if (Mathf.Abs(_player.Velocity.X) < 0.1f) {
                    _animatedSprite2D.Animation = "idle";
                } else {
                    _animatedSprite2D.Animation = "walk";
                    
                    _imageIndex += _playerMovement.SpeedX / 20f;
                    int frameCount = _animatedSprite2D.SpriteFrames.GetFrameCount("walk");
                    if (frameCount > 0) {
                        int frame = (int)Mathf.Abs(_imageIndex) % frameCount;
                        _animatedSprite2D.Frame = frame;
                    }
                }
            }
        }
    }
    public void OnJumpStarted() {
        _animatedSprite2D.Frame = 0;
    }
}
