using Godot;
using System;
using SMWP.Level.Physics;

public partial class TroopaFlyBlueMovement : BasicMovement {
    [Export] public float Height = 32f;

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        // 顶头 y 速度取绝对值
        if (MoveObject.IsOnCeiling()) {
            SpeedY = Mathf.Abs(SpeedY);
        }
    }

    public override void SetJumpSpeed() {
        if (!MoveObject.IsOnFloor()) return;
        //SpeedY = Mathf.Min(0f, -(25f / Mathf.Pow(Height, 3) - 10f / Mathf.Pow(Height, 2)));
        SpeedY = Mathf.Min(0f, JumpSpeed);
        Gravity = Mathf.Max(0.1f, 50f / (Height - 3f));
    }
}
