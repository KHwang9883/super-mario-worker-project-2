using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerAnimation : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private AnimatedSprite2D _ani = null!;
    [Export] private CharacterBody2D _player = null!;

    [Export] private SpriteFrames _marioSmall = null!;
    [Export] private SpriteFrames _marioSuper = null!;
    [Export] private SpriteFrames _marioFireball = null!;
    [Export] private SpriteFrames _marioBeetroot = null!;
    [Export] private SpriteFrames _marioLui = null!;
    [Export] private SpriteFrames _marioRaccoon = null!;
    
    [Export] private PackedScene _playerEffectScene = null!;
    
    private PlayerMovement _playerMovement = null!;
    private float _imageIndex;
    private int _frameCount;

    private AnimatedSprite2D? _invincibleEffect;

    private bool _hurting;
    private int _hurtAnimationTimer;
    private float _hurtEffectSinePhase;

    private int _hurtFlashCounter;

    private float _lastPlayerX;

    private bool _powerupChanging;
    public bool Fire;

    public override void _Ready() {
        _playerMovement = _playerMediator.GetNode<PlayerMovement>("PlayerMovement");
        _playerMovement.JumpStarted += OnJumpStarted;
        _invincibleEffect = _player.GetNode<AnimatedSprite2D>("InvincibleEffect");
        //_playerMediator.playerDieAndHurt.PlayerHurt += OnPlayerHurt;
        _lastPlayerX = _player.Position.X;
    }
    public override void _PhysicsProcess(double delta) {
        // 水管传送状态的动画处理
        if (_playerMediator.playerMovement.IsInPipeTransport) {
            _ani.Frame = 0;
            if (_ani.GetAnimation().Equals("shoot")) {
                _ani.Play("idle");
            }
            if (_playerMediator.playerMovement.IsInWater) {
                _ani.Play("swim");
            }
            if (_ani.GetAnimation().Equals("swim")
                && !_playerMediator.playerMovement.IsInWater) {
                _ani.Play("jump");
            }
            if (_playerMediator.playerMovement.PipeTransportDir
                is not (PlayerMovement.PipeTransportDirection.Left
                or PlayerMovement.PipeTransportDirection.Right)) return;
            _ani.FlipH = (_player.Position.X - _lastPlayerX < 0f);
            _lastPlayerX = _player.Position.X;
            return;
        }
        
        // 上帝模式飞行状态下动画禁用处理
        if (_playerMediator.playerGodMode.IsGodFly) {
            _ani.ProcessMode = ProcessModeEnum.Disabled;
            return;
        }
        _ani.ProcessMode = ProcessModeEnum.Inherit;
        
        // Powerup SpriteFrames
        if (!_hurting) {
            switch (_playerMediator.playerSuit.Suit) {
                case PlayerSuit.SuitEnum.Small:
                    _ani.SpriteFrames = _marioSmall;
                    break;
                case PlayerSuit.SuitEnum.Super:
                    _ani.SpriteFrames = _marioSuper;
                    break;
                case PlayerSuit.SuitEnum.Powered:
                    switch (_playerMediator.playerSuit.Powerup) {
                        case PlayerSuit.PowerupEnum.Fireball:
                            _ani.SpriteFrames = _marioFireball;
                            break;
                        case PlayerSuit.PowerupEnum.Beetroot:
                            _ani.SpriteFrames = _marioBeetroot;
                            break;
                        case PlayerSuit.PowerupEnum.Lui:
                            _ani.SpriteFrames = _marioLui;
                            break;
                        case PlayerSuit.PowerupEnum.Raccoon:
                            _ani.SpriteFrames = _marioRaccoon;
                            break;
                    }
                    break;
            }
        }
        
        // Starman
        if (_invincibleEffect != null) _invincibleEffect.Visible = _playerMediator.playerSuit.Starman;

        // Direction
        if (_playerMovement.SpeedX > 0f) {
            _ani.FlipH = false;
        } else if (_playerMovement.SpeedX < 0f) {
            _ani.FlipH = true;
        }
        
        // Walk Jump Idle Fire
        if (!_playerMovement.IsInWater) {
            if (!_player.IsOnFloor()) {
                // 浣熊装
                if (_playerMediator.playerSuit
                    is { Suit: PlayerSuit.SuitEnum.Powered, Powerup: PlayerSuit.PowerupEnum.Raccoon }
                    ) {
                    if (_playerMovement.RaccoonFall && !Fire) {
                        _ani.Play("fall");
                    }
                    if (_playerMovement.RaccoonAllowFly && !Fire) {
                        _ani.Play("fly");
                    }
                    if (_playerMovement is { RaccoonFall: false, RaccoonAllowFly: false } && !Fire) {
                        _ani.Animation = "jump";
                    }
                    if (Fire && _ani.Animation != "shoot") {
                        _ani.Play("shoot");
                    }
                }
                else {
                    _ani.Animation = "jump";
                }
            } else {
                if (Fire && _ani.SpriteFrames.HasAnimation("shoot")) {
                    _ani.Play("shoot");
                } else if (Mathf.Abs(_playerMovement.SpeedX) < 0.01f) {
                    _ani.Animation = "idle";
                } else {
                    if (_playerMediator.playerSuit
                        is { Suit: PlayerSuit.SuitEnum.Powered, Powerup: PlayerSuit.PowerupEnum.Raccoon }
                       && _playerMovement.RaccoonAllowFly
                       && _playerMovement.IsRaccoonDashAble()) {
                        _ani.Play("run");
                    }
                    else {
                        _ani.Animation = "walk";
                        if (!_hurting && !_powerupChanging) {
                            _imageIndex += _playerMovement.SpeedX / 10f;
                            _frameCount = _ani.SpriteFrames.GetFrameCount("walk");
                            if (_frameCount > 0) {
                                int frame = (int)Mathf.Abs(_imageIndex) % _frameCount;
                                _ani.Frame = frame;
                            }
                        }
                    }
                    
                }
            }
            if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small
                && _playerMovement.Crouched) {
                _ani.Animation = "crouch";
            }
        }
        // 水中动画处理
        else {
            if (_playerMediator.playerSuit is
                { Suit: PlayerSuit.SuitEnum.Powered, Powerup: PlayerSuit.PowerupEnum.Raccoon }
                && Fire) {
                _ani.Play("shoot");
            } else {
                if (!_player.IsOnFloor()) {
                    if (_ani.Animation != "swim" || _playerMovement.Jumped)
                        _ani.Play("swim");
                } else {
                    if (Fire && _ani.SpriteFrames.HasAnimation("shoot")) {
                        _ani.Play("shoot");
                    } else if (Mathf.Abs(_playerMovement.SpeedX) < 0.01f) {
                        _ani.Animation = "idle";
                    } else {
                        _ani.Animation = "walk";
                        if (!_hurting && !_powerupChanging) {
                            _imageIndex += _playerMovement.SpeedX / 20f;
                            _frameCount = _ani.SpriteFrames.GetFrameCount("walk");
                            if (_frameCount > 0) {
                                int frame = (int)Mathf.Abs(_imageIndex) % _frameCount;
                                _ani.Frame = frame;
                            }
                        }
                    }
                }
            }
            if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small
                && _playerMovement.Crouched) {
                _ani.Animation = "crouch";
            }
        }
        // Hurting Animation
        if (_hurting || _powerupChanging) {
            _hurtEffectSinePhase += 10f;
            _ani.Scale = new Vector2(1f, (float)(1f + Mathf.Sin(Mathf.DegToRad(_hurtEffectSinePhase)) / 3f));
            _hurtAnimationTimer++;
            if (_hurtAnimationTimer > 60) {
                _hurtAnimationTimer = 0;
                _hurtEffectSinePhase = 0f;
                _ani.Scale = new Vector2(1f, 1f);
                ProcessMode = ProcessModeEnum.Inherit;
                GetTree().Paused = false;
                _hurting = false;
                _powerupChanging = false;
            }
        }
        // Hurt Animation
        if (_playerMediator.playerDieAndHurt.IsHurtInvincible && !_hurting && !_powerupChanging) {
            _hurtFlashCounter++;
            _ani.Visible = ((_hurtFlashCounter / 4) % 2 == 0);
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
        _ani.Frame = 0;
    }
    public void OnPlayerHurt() {
        GetTree().Paused = true;
        ProcessMode = ProcessModeEnum.Always;
        _hurting = true;
    }
    public void OnPlayerInvincibleEnded() {
        _ani.Visible = true;
        _hurtFlashCounter = 0;
    }
    public void OnPLayerChangingSuit() {
        GetTree().Paused = true;
        ProcessMode = ProcessModeEnum.Always;
        _powerupChanging = true;
    }
    public void OnAnimationFinished() {
        if (_ani.Animation == "shoot") {
            Fire = false;
            GD.Print($"PlayerAnimation: Finished, Fire: {Fire}");
        }
    }
    public void OnPipeEntered() {
        _lastPlayerX = _player.Position.X;
    }
}
