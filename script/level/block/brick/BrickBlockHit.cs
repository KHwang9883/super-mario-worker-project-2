using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Player;

namespace SMWP.Level.Block.Brick;

public partial class BrickBlockHit : BlockHit {
    private PlayerSuit _playerSuit = null!;
    private bool _bumpable;
    private bool _breakable;
    
    protected override bool IsBumpable(Node2D collider) {
        if (collider.HasMeta("PlayerSuit")) {
            _playerSuit = (PlayerSuit)collider.GetMeta("PlayerSuit");
        }
        _bumpable = ((_playerSuit?.Suit == PlayerSuit.SuitEnum.Small) || collider.IsInGroup("beetroot"));
        return _bumpable;
    }
    protected override bool IsBreakable(Node2D collider) {
        if (collider.HasMeta("PlayerSuit")) {
            _playerSuit = (PlayerSuit)collider.GetMeta("PlayerSuit");
        }
        _breakable = ((_playerSuit?.Suit != PlayerSuit.SuitEnum.Small) || collider.IsInGroup("beetroot"));

        // Todo: if collider is IHardshell
        
        return _breakable;
    }
}
