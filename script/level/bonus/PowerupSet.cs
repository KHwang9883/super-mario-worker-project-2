using Godot;
using System;

namespace SMWP.Level.Bonus;

public partial class PowerupSet : Node {
    public enum PowerupEnum {
        Mushroom,
        FireFlower,
        Beetroot,
        Lui,
        Raccoon,
        SuperStar,
        LifeMushroom,
    }
    [Export] private Node2D _powerup = null!;
    [Export] public PowerupEnum PowerupType = PowerupEnum.Mushroom;
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        _parent.SetMeta("PowerupSet", this);
    }
    public void OnCollected() {
        _powerup.QueueFree();
    }
}
