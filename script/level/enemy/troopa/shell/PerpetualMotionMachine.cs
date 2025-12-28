using Godot;
using System;
using SMWP.Level.Physics;

public partial class PerpetualMotionMachine : Node2D {
    [Export] private ShellMovingInteraction? _shellMovingInteraction;
    [Export] private BasicMovement? _basicMovement;

    public override void _PhysicsProcess(double delta) {
        if (_shellMovingInteraction == null) {
            GD.PushError($"PerpetualMotionMachine: _shellMovingInteraction is null!");
            return;
        }
        if (_basicMovement == null) {
            GD.PushError($"PerpetualMotionMachine: _basicMovement is null!");
            return;
        }
        
        var results = _shellMovingInteraction.GetOverlapResult();
        var shell = _basicMovement.MoveObject;
        var originMask = shell.CollisionMask;
        shell.CollisionMask = 37;
        
        foreach (var result in results) {
            if (result is not PerpetualMotionMachineMarker) {
                continue;
            }
            // 上穿
            _basicMovement.SpeedY = 0f;
            //GD.Print($"撞到了吗？撞到了：{result}");
            var collideResult = shell.MoveAndCollide(
                new Vector2(_basicMovement.SpeedX, 0f), false, 0.09f);
            //GD.Print($"撞了个{collideResult}");
            while (collideResult is not null) {
                //GD.Print($"撞到了吗？撞到了，撞了个{collideResult}");
                shell.Position += Vector2.Up * 1.01f;
                shell.ForceUpdateTransform();
                collideResult = shell.MoveAndCollide(
                    new Vector2(_basicMovement.SpeedX, 0f), false, 0.09f);
            }
            shell.ResetPhysicsInterpolation();
        }
        
        shell.CollisionMask = originMask;
    }
}
