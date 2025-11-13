using Godot;
using System;
using SMWP.Level.Physics;

public partial class TroopaFlyBlueMovement : BasicMovement {
    [Export] public float Height = 32f;

    public override void SetJumpSpeed() {
        if (!MoveObject.IsOnFloor()) return;
        SpeedY = Mathf.Min(0f, JumpSpeed);
        Gravity = SpeedY / 5f;
    }
}
