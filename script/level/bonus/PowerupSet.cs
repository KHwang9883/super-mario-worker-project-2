using Godot;
using System;
using SMWP.Interface;

public partial class PowerupSet : Node {
    public enum PowerupEnum {
        Mushroom,
        FireFlower,
        Beetroot,
        Lui
    }
    [Export] private Node2D _powerup = null!;
    [Export] public PowerupEnum PowerupType = PowerupEnum.Mushroom;
    
    public void OnCollected() {
        _powerup.QueueFree();
    }
}
