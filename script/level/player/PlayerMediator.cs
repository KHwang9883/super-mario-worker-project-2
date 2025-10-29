using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerMediator : Node {
    [Export] public PlayerMovement playerMovement { get; private set; } = null!;
    [Export] public PlayerDie playerDie { get; private set; } = null!;
    [Export] public PlayerAnimation playerAnimation { get; private set; } = null!;
    [Export] public PlayerSuit playerSuit { get; private set; } = null!;
}
