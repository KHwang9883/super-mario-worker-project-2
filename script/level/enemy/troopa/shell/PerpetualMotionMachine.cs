using Godot;
using System;
using SMWP.Level.Physics;

public partial class PerpetualMotionMachine : Node2D {
    [Export] private ShellMovingInteraction? _shellMovingInteraction;
    [Export] private BasicMovement? _basicMovement;
    
    [Export] private float _moveUpDistance = 4f;

    public override void _PhysicsProcess(double delta) {
        if (_shellMovingInteraction == null) {
            GD.PushError($"PerpetualMotionMachine: _shellMovingInteraction is null!");
            return;
        }
        if (_basicMovement == null) {
            GD.PushError($"PerpetualMotionMachine: _basicMovement is null!");
            return;
        }

        if (_basicMovement.SpeedY <= 0f) {
            return;
        }
        
        var shell = _basicMovement.MoveObject;
        var originMask = shell.CollisionMask;
        shell.CollisionMask = 4101;
        _basicMovement.MoveObject.ForceUpdateTransform();
        var results = _shellMovingInteraction.GetOverlapResult();
        
        //GD.Print($"Overlap Results: {results}");
        
        foreach (var result in results) {
            if (result is not PerpetualMotionMachineMarker) {
                continue;
            }
            // 上穿
            _basicMovement.SpeedY = 0f;
            //GD.Print($"Collided with PerpetualMotionMachineMarker: {result}");
            var collideResult = shell.MoveAndCollide(
                new Vector2(_basicMovement.SpeedX, 0f), true, 0.09f);
            //GD.Print($"MoveAndCollide Result: {collideResult}");
            // 循环有限次，防止意外情况
            for (var i = 0; i < 600; i++) {
                if (collideResult != null) {
                    shell.Position += Vector2.Up * _moveUpDistance;
                    shell.ForceUpdateTransform();
                    collideResult = shell.MoveAndCollide(
                        new Vector2(_basicMovement.SpeedX, 0f), true, 0.09f);
                } else {
                    break;
                }
            }
            shell.ResetPhysicsInterpolation();
        }
        
        shell.CollisionMask = originMask;
    }
}
