using Godot;
using System;
using SMWP.Level.Block;

public partial class QuestionBlock : BlockHit {
    [Export] public PackedScene SproutItemScene = GD.Load<PackedScene>("uid://bb7ac4wrewh71");
    [Export] private AnimatedSprite2D _ani = null!;

    protected override void OnBlockBump() {
        base.OnBlockBump();
        _ani.Play("hit");
        OnSproutItem();
    }
    public void OnSproutItem() {
        var sproutItem = SproutItemScene.Instantiate<Node2D>();
        sproutItem.Position = Parent.Position;
        Parent.AddSibling(sproutItem);
    }
}
