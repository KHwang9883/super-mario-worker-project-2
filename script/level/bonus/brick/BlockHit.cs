using Godot;
using System;
using SMWP.Interface;

namespace SMWP.Level.Bonus.Brick;

public partial class BlockHit : Node, IBlockHittable{
    [Signal]
    public delegate void BlockHitSucceedEventHandler(Node2D collider);

    private enum BlockEnum {
        Brick,
        Question,
        Coin,
        Switch,
    }
    [Export] private BlockEnum _blockType = BlockEnum.Brick;
    private enum VisibleEnum {
        Visible,
        Invisible,
    }
    [Export] private VisibleEnum _visibleType = VisibleEnum.Visible;
    
    // TEST
    public void OnBlockHit(Node2D collider) {
        GD.Print("今天是最臭的一天");
    }
}
