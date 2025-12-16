using Godot;
using System;
using SMWP.Level.Physics;
using SMWP.Level.Sound;

public partial class HammerBroMovement : BasicMovement {
    [Export] private int _jumpTime = 100;
    [Export] private bool _allowAngryMode;

    private int _walkTimer1;
    private int _walkTimer2;
    private int _jumpTimer;
    private RandomNumberGenerator _rng = new();
    
    private int _angryModeTimer;
    private bool _angryMode;
    
    public enum BroJumpFallStatus { InWall, Free }
    private BroJumpFallStatus _fallStatus = BroJumpFallStatus.Free;
    
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
}
