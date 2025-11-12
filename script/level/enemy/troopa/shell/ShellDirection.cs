using Godot;
using System;
using SMWP.Level.Physics;

public partial class ShellDirection : BasicMovement {
    public override void SetMovementDirection() {
        if (!InitiallyFaceToPlayer) return;
        var player = MoveObject.HasMeta("InteractingObject") ?
            (Node2D)MoveObject.GetMeta("InteractingObject") : null;
        if (player != null && player.IsInGroup("player")) {
            if (MoveObject.GlobalPosition.X < player?.GlobalPosition.X) {
                SpeedX = -Mathf.Abs(SpeedX);
            } else if (MoveObject.GlobalPosition.X > player?.GlobalPosition.X) {
                SpeedX = Mathf.Abs(SpeedX);
            }
        } else {
            if (MoveObject.GlobalPosition.X < Player?.GlobalPosition.X) {
                SpeedX = Mathf.Abs(SpeedX);
            } else if (MoveObject.GlobalPosition.X > Player?.GlobalPosition.X) {
                SpeedX = -Mathf.Abs(SpeedX);
            }
        }
    }
}
