using Godot;
using System;
using SMWP.Level.Player;

namespace SMWP.Level.Player;

public partial class PlayerAnimation : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private CharacterBody2D _player = null!;

    [Export] private SpriteFrames _marioSmall = null!;
    [Export] private SpriteFrames _marioSuper = null!;
    [Export] private SpriteFrames _marioFireball = null!;
    [Export] private SpriteFrames _marioBeetroot = null!;
    [Export] private SpriteFrames _marioLui = null!;
    
    [Export] private PackedScene _playerEffectScene = null!;
    
    private PlayerMovement _playerMovement = null!;
    private float _imageIndex;
    private int _frameCount;

    private AnimatedSprite2D? _invincibleEffect;

    private bool _hurting;
    private int _hurtAnimationTimer;
    private float _hurtEffectSinePhase;

    private int _hurtFlashCounter;

    private bool _powerupChanging;
    public bool Fire;

    public override void _Ready() {
        _playerMovement = _playerMediator.GetNode<PlayerMovement>("PlayerMovement");
        _playerMovement.JumpStarted += OnJumpStarted;
        _invincibleEffect = _player.GetNode<AnimatedSprite2D>("InvincibleEffect");
        //_playerMediator.playerDieAndHurt.PlayerHurt += OnPlayerHurt;
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
        
        // Starman
        if (_invincibleEffect != null) _invincibleEffect.Visible = _playerMediator.playerSuit.Starman;

        // Direction
        if (_playerMovement.SpeedX > 0f) {
            _animatedSprite2D.FlipH = false;
        } else if (_playerMovement.SpeedX < 0f) {
            _animatedSprite2D.FlipH = true;
        }
        
        // Walk Jump Idle Fire
        if (!_playerMovement.IsInWater) {
            if (!_player.IsOnFloor()) {
                _animatedSprite2D.Animation = "jump";
            } else {
                if (Fire && _animatedSprite2D.SpriteFrames.HasAnimation("shoot")) {
                    _animatedSprite2D.Play("shoot");
                } else if (Mathf.Abs(_playerMovement.SpeedX) < 0.01f) {
                    _animatedSprite2D.Animation = "idle";
                } else {
                    _animatedSprite2D.Animation = "walk";
                    if (!_hurting && !_powerupChanging) {
                        _imageIndex += _playerMovement.SpeedX / 10f;
                        _frameCount = _animatedSprite2D.SpriteFrames.GetFrameCount("walk");
                        if (_frameCount > 0) {
                            int frame = (int)Mathf.Abs(_imageIndex) % _frameCount;
                            _animatedSprite2D.Frame = frame;
                        }
                    }
                }
            }
            if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small
                && _playerMovement.Crouched) {
                _animatedSprite2D.Animation = "crouch";
            }
        } else {
            if (!_player.IsOnFloor()) {
                _animatedSprite2D.Play("swim");
            } else {
                if (Fire && _animatedSprite2D.SpriteFrames.HasAnimation("shoot")) {
                    _animatedSprite2D.Play("shoot");
                } else if (Mathf.Abs(_playerMovement.SpeedX) < 0.01f) {
                    _animatedSprite2D.Animation = "idle";
                } else {
                    _animatedSprite2D.Animation = "walk";
                    if (!_hurting && !_powerupChanging) {
                        _imageIndex += _playerMovement.SpeedX / 20f;
                        _frameCount = _animatedSprite2D.SpriteFrames.GetFrameCount("walk");
                        if (_frameCount > 0) {
                            int frame = (int)Mathf.Abs(_imageIndex) % _frameCount;
                            _animatedSprite2D.Frame = frame;
                        }
                    }
                }
            }
            if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small
                && _playerMovement.Crouched) {
                _animatedSprite2D.Animation = "crouch";
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
        if (_playerMediator.playerDieAndHurt.IsHurtInvincible && !_hurting && !_powerupChanging) {
            _hurtFlashCounter++;
            _animatedSprite2D.Visible = ((_hurtFlashCounter / 4) % 2 == 0);
        }
        
        // 尝试性加入玩家阴影特效，但是参数不会调，所以暂时禁用
        // Player Effect
        /*
        if (_hurtAnimationTimer == 0) {
            var playerEffect = _playerEffectScene.Instantiate<Sprite2D>();
            playerEffect.Texture = _animatedSprite2D.SpriteFrames.GetFrameTexture(_animatedSprite2D.Animation, _animatedSprite2D.Frame);
            _player.AddSibling(playerEffect);
            playerEffect.GlobalPosition = new Vector2(_animatedSprite2D.GlobalPosition.X, _animatedSprite2D.GlobalPosition.Y);
            playerEffect.ResetPhysicsInterpolation();
            playerEffect.FlipH = _animatedSprite2D.FlipH;
            playerEffect.Modulate = playerEffect.Modulate with { A = 0.1f };
            playerEffect.Offset = _animatedSprite2D.Offset;
        }
        */
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
    public void OnAnimationFinished() {
        if (_animatedSprite2D.Animation == "shoot") {
            Fire = false;
        }
    }
}
