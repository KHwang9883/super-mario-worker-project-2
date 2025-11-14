using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Interface;

public partial class ThwompInteraction : Node {
    [Export] private ThwompMovement _thwompMovement = null!;
    private CharacterBody2D? _parent;

    public override void _Ready() {
        _parent ??= (CharacterBody2D)GetParent();
        _parent.SetMeta("ThwompInteraction", this);
    }
    public void OnThwompBlockHit() {
        if (_parent == null) return;
        
        /*for (var i = 0; i < 20; i++) {
            Node? interactionWithBlockNode = null;
            
            var blockCollider = _parent.MoveAndCollide(new Vector2(0f, 1f), true, 0.01f)?.GetCollider();
            GD.Print(blockCollider);
            if (blockCollider is not StaticBody2D staticBody2D) return;
            if (staticBody2D.HasMeta("InteractionWithBlock")) {
                interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
            }
            if (interactionWithBlockNode is not BlockHit blockHit) return;
            blockHit.OnBlockHit(_parent);
        }*/
    }
    public void OnThwompBlockHit(Node2D body) {
        if (_parent == null) return;
        //if (_thwompMovement.CurrentState != ThwompMovement.ThwompState.Grounded) return;
        Node? interactionWithBlockNode = null;
            
        if (body is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
                interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        blockHit.OnBlockHit(_parent);
    }
}
