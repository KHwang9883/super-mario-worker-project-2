using Godot;
using System;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.Beetroot;

public partial class BeetrootMovement : BasicMovement {
    [Signal]
    public delegate void HitBlockEventHandler();
    
    public float Direction { get; set; } = 1f;
    public int BounceCount;
    private float _speedXFactor = 1f;
    private ShapeCast2D _shapeCast2D = null!;

    public override void _Ready() {
        SpeedX = Mathf.Abs(SpeedX) * Direction;
        _shapeCast2D = GetParent().GetNode<ShapeCast2D>("AreaBodyCollision");
    }
    public override void _PhysicsProcess(double delta) {
        if (BounceCount < 4) {
            if (MoveObject.IsOnWall() || MoveObject.IsOnFloor()) {
                Bounce();
            }
        } else {
            // 遗憾离场
            _speedXFactor = -1.5f;
            MoveObject.SetCollisionMask(0);
            _shapeCast2D.SetCollisionMask(0);
        }

        SpeedX = (2f + _speedXFactor) * Direction;
        SpeedY += Gravity;
        MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
        MoveObject.MoveAndSlide();
    }
    public void Bounce() {
        Direction = 0f - Direction;
        BounceCountAdd();
        _speedXFactor = BounceCount;
        SpeedY = JumpSpeed;
        EmitSignal(SignalName.HitBlock);
    }
    public void BounceCountAdd() {
        BounceCount++;
    }
}
