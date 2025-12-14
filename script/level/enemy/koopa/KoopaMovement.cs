using Godot;
using System;
using SMWP.Level.Physics;
using SMWP.Level.Sound;

public partial class KoopaMovement : BasicMovement {
    [Export] private PackedScene _projectileScene = GD.Load<PackedScene>("uid://cqqq3dudrdi2l");
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    [Export] private ContinuousAudioStream2D? _flameSound;
    [Export] private int _jumpTime = 100;
    [Export] private float _flameTime = 250f;

    private int _walkTimer1;
    private int _walkTimer2;
    private int _jumpTimer;
    private float _flameTimer;
    private float _flameTimerBoost;
    private float _flameFixedPositionY;
    private RandomNumberGenerator _rng = new();
    
    public enum KoopaJumpFallStatus { InWall, Free }
    private KoopaJumpFallStatus _fallStatus = KoopaJumpFallStatus.Free;
    
    public override void _Ready() {
        base._Ready();
        _flameTimerBoost = _rng.RandfRange(0f, 0.2f);
        _flameFixedPositionY = MoveObject.Position.Y + 32f;
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
            _fallStatus = IsInBlock() ? KoopaJumpFallStatus.InWall : KoopaJumpFallStatus.Free;
            
            if (_fallStatus == KoopaJumpFallStatus.InWall && !IsInBlock()) {
                _fallStatus = KoopaJumpFallStatus.Free;
            }
            
            if (_fallStatus == KoopaJumpFallStatus.Free) {
                MoveObject.SetCollisionMask(5);
            }
        }

        // Flame Status
        _flameTimer += 1f + _flameTimerBoost;
        if (_flameTimer > _flameTime * 0.6f) {
            if (_animatedSprite2D?.Animation != "flame") {
                _animatedSprite2D?.Play("flame");
            }
        }
        if (_flameTimer > _flameTime) {
            Launch();
            _animatedSprite2D?.Play("default");
            _flameSound?.Play();
            _flameTimer = 0f;
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
        projectile.SetPosition(MoveObject.Position + Vector2.Up * 16f);
        // 记录库巴的初始 y 位置，还原原版“半格火”特性
        projectile.SetMeta("FixedPositionY", _flameFixedPositionY);
        MoveObject.AddSibling(projectile);
    }
}
