using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Physics;

public partial class ShellMovingBlockHitInteraction : Node {
    [Export] private BasicMovement _basicMovement = null!;
    
    private CharacterBody2D? _shellHard;
    private Vector2 _motion;

    public override void _Ready() {
        _shellHard ??= (CharacterBody2D)GetParent();
        _shellHard.SetMeta("ShellBlockHitInteraction", this);
    }
    public override void _PhysicsProcess(double delta) {
        if (_shellHard == null) return;
        
        Node? interactionWithBlockNode = null;
        var blockCollider =
            _shellHard.MoveAndCollide(
                new Vector2(1f * Mathf.Sign(_basicMovement.SpeedX), 0f), true)?.GetCollider();
        //GD.Print(blockCollider);
        if (blockCollider is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        
        // 隐藏砖不触发
        if (blockHit.Hidden) return;

        blockHit.OnBlockHit(_shellHard);
    }
    public void OnMoveProcess(Vector2 velocity) {
        _motion = velocity;
    }
}
