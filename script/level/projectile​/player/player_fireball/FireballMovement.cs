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
        if (!MoveObject.IsOnWall()) return;
        EmitSignal(SignalName.FireballExplode);
        MoveObject.QueueFree();
    }
}
