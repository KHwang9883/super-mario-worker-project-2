using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Interface;

public partial class ThwompInteraction : Area2D {
    private Node2D? _parent;
    private CharacterBody2D? _thwomp;

    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _thwomp ??= (CharacterBody2D)_parent.GetMeta("Thwomp");
        _thwomp.SetMeta("ThwompInteraction", this);
    }
    public void OnThwompBlockHit(Node2D body) {
        Node? interactionWithBlockNode = null;
        
        Callable.From(() => {
            _parent?.QueueFree();
        }).CallDeferred();
        
        if (body is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        
        // 隐藏砖不触发
        if (blockHit.Hidden) return;
        
        if (_thwomp != null) blockHit.OnBlockHit(_thwomp);
    }
}
