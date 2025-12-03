using Godot;
using System;
using SMWP.Level.Physics;
using SMWP.Level.Player;

namespace SMWP.Level.Projectile.Player.Beetroot;

public partial class BeetrootMovement : BasicMovement {
    [Signal]
    public delegate void HitBlockEventHandler();
    
    public float Direction { get; set; } = 1f;
    public int BounceCount;
    private float _speedXFactor = 1f;
    private ShapeCast2D _shapeCast2D = null!;

    [Export] public bool IsMwBeetroot;
    [Export] public float JumpSpeedMwBeetroot = -7f;
    [Export] public bool CanMwBeetrootClimbWallUpwards = true;

    public override void _Ready() {
        SpeedX = Mathf.Abs(SpeedX) * Direction;
        _shapeCast2D = GetParent().GetNode<ShapeCast2D>("AreaBodyCollision");
        
        if (!IsMwBeetroot) return;
        Callable.From(() => {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (!player.HasMeta("PlayerMovement")) return;
            var playerMovement = (PlayerMovement)player.GetMeta("PlayerMovement");
            SpeedX = Mathf.Max(SpeedX, Mathf.Abs(playerMovement.SpeedX)) * Direction;
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        // MW 甜菜
        if (IsMwBeetroot) {
            if (BounceCount < 4) {
                if (CanMwBeetrootClimbWallUpwards) {
                    while (MoveObject.MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
                        MoveObject.Position += Vector2.Up;
                        MoveObject.ForceUpdateTransform();
                    }
                }
                if (MoveObject.IsOnFloor()) {
                    Bounce();
                }
                if (MoveObject.IsOnWall()) {
                    Direction = -Direction;
                }
            } else {
                // 遗憾离场
                _speedXFactor = -1.5f;
                MoveObject.SetCollisionMask(0);
                _shapeCast2D.SetCollisionMask(0);
            }
            
            SpeedX = Mathf.Abs(SpeedX) * Direction;
            SpeedY += Gravity;
            MoveObject.Velocity = new Vector2(SpeedX * FramerateOrigin, SpeedY * FramerateOrigin);
        
            MoveObject.MoveAndSlide();
        }
        
        // MF 甜菜
        else {
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
    }
    public void Bounce() {
        if (IsMwBeetroot) {
            SpeedY = JumpSpeed;
            BounceCountAdd();
        } else {
            EmitSignal(SignalName.HitBlock);
            Direction = 0f - Direction;
            BounceCountAdd();
            _speedXFactor = BounceCount;
            SpeedY = JumpSpeedMwBeetroot;
        }
    }
    public void BounceCountAdd() {
        BounceCount++;
    }
}
