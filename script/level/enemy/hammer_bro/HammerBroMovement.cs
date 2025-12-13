using Godot;
using System;
using SMWP.Level.Physics;
using SMWP.Level.Sound;

public partial class HammerBroMovement : BasicMovement {
    [Export] private PackedScene _projectileScene = GD.Load<PackedScene>("uid://bcineprwgpk4w");
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    [Export] private ContinuousAudioStream2D? _shootSound;
    [Export] private int _jumpTime = 100;
    [Export] private float _shootTime = 250f;
    [Export] private bool _allowAngryMode;

    private int _walkTimer1;
    private int _walkTimer2;
    private int _jumpTimer;
    private float _shootTimer;
    private float _shootTimerBoost;
    private RandomNumberGenerator _rng = new();
    
    private int _angryModeTimer;
    private bool _angryMode;
    
    public enum BroJumpFallStatus { InWall, Free }
    private BroJumpFallStatus _fallStatus = BroJumpFallStatus.Free;
    
    public override void _Ready() {
        base._Ready();
        _shootTimerBoost = _rng.RandfRange(0f, 0.2f);
    }
    public override void _PhysicsProcess(double delta) {
        // Walk Status
        if (IsOnWall()) {
            MoveObject.Position -= new Vector2(SpeedX, 0f);
            SpeedX = 0f;
        }
        
        if (!IsInBlock() && !IsOnWall()) {
            if (_walkTimer1 == 0 && _walkTimer2 == 0) {
                _walkTimer1 = 10 + _rng.RandiRange(0, 150);
                _walkTimer2 = _walkTimer1;
            }
            if (_walkTimer1 > 0) {
                SpeedX = 1f;
                _walkTimer1 -= 1;
            }
            if (_walkTimer1 == 0 && _walkTimer2 > 0) {
                SpeedX = -1f;
                _walkTimer2 -= 1;
            }
        }
        
        // Jump Status
        if (MoveObject.IsOnFloor()) {
            _jumpTimer++;
            SpeedY = 0f;
            if (_jumpTimer >= _jumpTime) {
                _jumpTimer = 0;
                SetJumpSpeed();
                MoveObject.SetCollisionMask(0);
            }
        }
        
        if (SpeedY > Gravity && !MoveObject.IsOnFloor()) {
            _fallStatus = IsInBlock() ? BroJumpFallStatus.InWall : BroJumpFallStatus.Free;
            
            if (_fallStatus == BroJumpFallStatus.InWall && !IsInBlock()) {
                _fallStatus = BroJumpFallStatus.Free;
            }
            
            if (_fallStatus == BroJumpFallStatus.Free) {
                MoveObject.SetCollisionMask(5);
            }
        }

        // Shoot Status
        _shootTimer += 1f + _shootTimerBoost;
        if (_shootTimer > _shootTime * 0.6f) {
            if (_animatedSprite2D?.Animation != "shoot") {
                _animatedSprite2D?.Play("shoot");
            }
        }
        if (_shootTimer > _shootTime) {
            Launch();
            _animatedSprite2D?.Play("default");
            _shootSound?.Play();
            _shootTimer = 0f;
        }
        
        // Angry Mode
        if (_allowAngryMode) {
            _angryModeTimer++;
            if (_angryModeTimer > 2500) {
                _angryMode = true;
            }
        }
        if (_angryMode && Player != null) {
            if (MoveObject.Position.X > Player.Position.X) {
                SpeedX = -2f;
            } else {
                SpeedX = 2f;
            }
        }
        
        SpeedYProcess();
        
        ApplySpeed();

        Move();
    }
    public bool IsInBlock() {
        var originCollisionMask = MoveObject.GetCollisionMask();
        MoveObject.SetCollisionMask(5);
        bool isInBlock = (MoveObject.MoveAndCollide(Vector2.Zero, true, 0.01f) != null);
        MoveObject.SetCollisionMask(originCollisionMask);
        return isInBlock;
    }

    public bool IsOnWall() {
        var originCollisionMask = MoveObject.GetCollisionMask();
        MoveObject.SetCollisionMask(5);
        bool isOnWall =
            (MoveObject.MoveAndCollide(new Vector2(Mathf.Sign(SpeedX) * 2f, 0f), true, 0.01f) != null);
        MoveObject.SetCollisionMask(originCollisionMask);
        return isOnWall;
    }
    public void Launch() {
        var projectile = _projectileScene.Instantiate<Node2D>();
        projectile.SetPosition(MoveObject.Position + Vector2.Up * 10f);
        MoveObject.AddSibling(projectile);
    }
}
