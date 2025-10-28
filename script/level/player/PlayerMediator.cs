using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerMediator : Node {
    [Export] public PlayerMovement playerMovement { get; private set; }
    [Export] public PlayerDie playerDie { get; private set; }
}
