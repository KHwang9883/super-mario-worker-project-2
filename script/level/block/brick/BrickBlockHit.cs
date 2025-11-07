using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Player;

namespace SMWP.Level.Block.Brick;

public partial class BrickBlockHit : BlockHit {
    [Signal]
    public delegate void BlockShatterEventHandler();
    
    private bool _bump;

    public override void OnBlockHit(Node2D collider) {
        if (IsHittable(collider)) {
            if (!_bump) {
                _bump = true;
                EmitSignal(BlockHit.SignalName.BlockBump);
            }
        } else {
            EmitSignal(SignalName.BlockShatter);
            Parent.QueueFree();
        }
    }

    protected override bool IsHittable(Node2D collider) {
        if (collider.GetNodeOrNull("PlayerMediator/PlayerSuit") is PlayerSuit playerSuit) {
            return playerSuit.Suit == PlayerSuit.SuitEnum.Small;
        }
        return false;
    }

    public override void OnBumped() {
        _bump = false;
    }
}
