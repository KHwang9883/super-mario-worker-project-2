using Godot;
using System;
using SMWP.Level.Physics;

public partial class HammerHammerBroMovement : BasicMovement {
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        MoveObject.RotationDegrees += 10f;
    }
}
