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
    
    private PlayerMovement _playerMovement = null!;
    private float _imageIndex;

    private bool _hurting;
    private int _hurtAnimationTimer;
    private float _hurtEffectSinePhase;

    private int _hurtFlashCounter;

    private bool _powerupChanging;

    public override void _Ready() {
        _playerMovement = _playerMediator.GetNode<PlayerMovement>("PlayerMovement");
        _playerMovement.JumpStarted += OnJumpStarted;
        _playerMediator.playerDieAndHurt.PlayerHurt += OnPlayerHurt;
    }
    public override void _PhysicsProcess(double delta) {
        // Powerup SpriteFrames
        if (!_hurting) {
            switch (_playerMediator.playerSuit.Suit) {
                case PlayerSuit.SuitEnum.Small:
                    _animatedSprite2D.SpriteFrames = _marioSmall;
                    break;
                case PlayerSuit.SuitEnum.Super:
                    _animatedSprite2D.SpriteFrames = _marioSuper;
                    break;
                case PlayerSuit.SuitEnum.Powered:
                    switch (_playerMediator.playerSuit.Powerup) {
                        case PlayerSuit.PowerupEnum.Fireball:
                            _animatedSprite2D.SpriteFrames = _marioFireball;
                            break;
                        case PlayerSuit.PowerupEnum.Beetroot:
                            _animatedSprite2D.SpriteFrames = _marioBeetroot;
                            break;
                        case PlayerSuit.PowerupEnum.Lui:
                            _animatedSprite2D.SpriteFrames = _marioLui;
                            break;
                    }
                    break;
            }
        }

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
                    if (!_hurting) {
                        _imageIndex += _playerMovement.SpeedX / 10f;
                        int frameCount = _animatedSprite2D.SpriteFrames.GetFrameCount("walk");
                        if (frameCount > 0) {
                            int frame = (int)Mathf.Abs(_imageIndex) % frameCount;
                            _animatedSprite2D.Frame = frame;
                        }
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
                    if (!_hurting && !_powerupChanging) {
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
        // Hurting Animation
        if (_hurting || _powerupChanging) {
            _hurtEffectSinePhase += 10f;
            _animatedSprite2D.Scale = new Vector2(1f, (float)(1f + Mathf.Sin(Mathf.DegToRad(_hurtEffectSinePhase)) / 3f));
            _hurtAnimationTimer++;
            if (_hurtAnimationTimer > 60) {
                _hurtAnimationTimer = 0;
                _hurtEffectSinePhase = 0f;
                _animatedSprite2D.Scale = new Vector2(1f, 1f);
                ProcessMode = ProcessModeEnum.Inherit;
                GetTree().Paused = false;
                _hurting = false;
                _powerupChanging = false;
            }
        }
        // Hurt Animation
        if (_playerMediator.playerDieAndHurt.IsInvicible && !_hurting && !_powerupChanging) {
            _hurtFlashCounter++;
            _animatedSprite2D.Visible = ((_hurtFlashCounter / 4) % 2 == 0);
        }
    }
    public void OnJumpStarted() {
        _animatedSprite2D.Frame = 0;
    }
    public void OnPlayerHurt() {
        GetTree().Paused = true;
        ProcessMode = ProcessModeEnum.Always;
        _hurting = true;
    }
    public void OnPlayerInvincibleEnded() {
        _animatedSprite2D.Visible = true;
        _hurtFlashCounter = 0;
    }
    public void OnPLayerChangingSuit() {
        GetTree().Paused = true;
        ProcessMode = ProcessModeEnum.Always;
        _powerupChanging = true;
    }
}
