using Godot;
using System;
using SMWP.Level.Physics;

public partial class ShellStaticMovement : BasicMovement {
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (MoveObject.IsOnWall()) {
            SpeedX = 0f;
        }
        if (MoveObject.IsOnCeiling()) {
            SpeedY = 0f;
        }
        if (MoveObject.IsOnFloor()) {
            SpeedX = 0f;
            SpeedY = 0f;
        }
    }
}
