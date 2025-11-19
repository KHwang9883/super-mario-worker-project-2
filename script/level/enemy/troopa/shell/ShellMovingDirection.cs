using Godot;
using System;
using SMWP.Level.Physics;

public partial class ShellMovingDirection : BasicMovement {
    public override void SetMovementDirection() {
        if (!InitiallyFaceToPlayer) return;
        // 被踩后的方向初始化设置
        var player = MoveObject.HasMeta("InteractingObject") ?
            (Node2D)MoveObject.GetMeta("InteractingObject") : null;
        if (player != null && player.IsInGroup("player")) {
            if (MoveObject.GlobalPosition.X < player?.GlobalPosition.X) {
                SpeedX = -Mathf.Abs(SpeedX);
            } else if (MoveObject.GlobalPosition.X > player?.GlobalPosition.X) {
                SpeedX = Mathf.Abs(SpeedX);
            }
        } else {
            // metadata中没有玩家，使用默认的方向设置判断
            if (MoveObject.GlobalPosition.X < Player?.GlobalPosition.X) {
                SpeedX = Mathf.Abs(SpeedX);
            } else if (MoveObject.GlobalPosition.X > Player?.GlobalPosition.X) {
                SpeedX = -Mathf.Abs(SpeedX);
            }
        }
    }
}
