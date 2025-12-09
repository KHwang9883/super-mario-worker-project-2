using Godot;
using System;
using System.Runtime.InteropServices.ComTypes;
using SMWP.Level.Block;
using SMWP.Level.Player;

public partial class QuestionBlock : BlockHit {
    [Export] public PackedScene SproutItemScene = GD.Load<PackedScene>("uid://bb7ac4wrewh71");
    [Export] public PackedScene? SproutItemAdvancedScene;
    [Export] private AnimatedSprite2D _ani = null!;

    private PlayerSuit? _playerSuit;

    public override void _Ready() {
        base._Ready();
        
        var player = GetTree().GetFirstNodeInGroup("player");
        if (player.HasMeta("PlayerSuit")) {
            _playerSuit ??= (PlayerSuit)player.GetMeta("PlayerSuit");
        }
    }

    protected override void OnBlockBump() {
        base.OnBlockBump();
        _ani.Play("hit");
        OnSproutItem();
    }
    public void OnSproutItem() {
        if (_playerSuit == null) {
            GD.PushError($"{this}: _playerSuit is null!");
            return;
        }
        if (Parent == null) {
            GD.PushError($"{this}: Parent is null!");
            return;
        }
            
        Callable.From(() => {
            // 小个子且问号二级状态槽位无道具
            if (_playerSuit.Suit == PlayerSuit.SuitEnum.Small || SproutItemAdvancedScene == null) {
                var sproutItem = SproutItemScene.Instantiate<Node2D>();
                sproutItem.Position = Parent.Position;
                Parent.AddSibling(sproutItem);
            }
            // 二级状态
            else {
                var sproutItemAdvanced = SproutItemAdvancedScene.Instantiate<Node2D>();
                sproutItemAdvanced.Position = Parent.Position;
                Parent.AddSibling(sproutItemAdvanced);
            }
        }).CallDeferred();
    }
}
