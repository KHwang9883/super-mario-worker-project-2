using Godot;
using System;
using SMWP.Level.Player;

namespace SMWP.Level.Player;

public partial class PlayerSuit : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    
    public enum SuitEnum {
        Small,
        Super,
        Powered
    }
    [Export] public SuitEnum Suit = SuitEnum.Small;
    public enum PowerupEnum {
        Fireball,
        Beetroot,
        Lui
    }
    [Export] public PowerupEnum Powerup = PowerupEnum.Fireball;
}
