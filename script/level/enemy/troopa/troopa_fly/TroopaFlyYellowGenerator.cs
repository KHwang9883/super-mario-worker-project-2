using Godot;
using System;

public partial class TroopaFlyYellowGenerator : Node2D {
    [Export] private PackedScene _troopaFlyGoldScene = GD.Load<PackedScene>("uid://crqummcbk1e7g");
    
    [Export] public float Radius;
    [Export] public float Angle;
    [Export] public int Amount;
    [Export] public bool Direction;

    public override void _Ready() {
        Visible = false;
        
        Callable.From(() => {
            for (var i = 0; i < Amount; i++) {
                var troopaFlyGold = _troopaFlyGoldScene.Instantiate<Node2D>();
                var troopaFlyGoldMovement = troopaFlyGold.GetNode<TroopaFlyYellowMovement>("TroopaFlyYellowMovement");
                troopaFlyGoldMovement.Radius = Radius;
                troopaFlyGoldMovement.Angle = Angle + 90f + (360f / Amount) * i;
                troopaFlyGoldMovement.Direction = Direction ? -1 : 1;
                troopaFlyGold.Position = Position;
                AddSibling(troopaFlyGold);
            }
        }).CallDeferred();
    }
}
