using Godot;
using System;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.PlayerFireball;

public partial class FireballMovement : BasicMovement {
    [Signal]
    public delegate void FireballExplodeEventHandler();
    public float Direction { get; set; } = 1f;
    
    public override void _Ready() {
        if (MoveObject.IsInGroup("fireball")) {
            SpeedX = Mathf.Abs(SpeedX) * Direction;
        } else {
            base._Ready();
        }
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (!MoveObject.IsOnWall() && MoveObject.MoveAndCollide(Vector2.Zero, true) == null) return;
        // 延迟是为了给火球检测冰块留足时间
        Callable.From(() => {
            EmitSignal(SignalName.FireballExplode);
            MoveObject.QueueFree();
        }).CallDeferred();
    }
}
