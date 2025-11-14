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
    public void OnThwompBlockHit(Node2D body) {
        if (_parent == null) return;
        Node? interactionWithBlockNode = null;
            
        if (body is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
                interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        blockHit.OnBlockHit(_parent);
    }
}
