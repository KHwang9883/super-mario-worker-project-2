using Godot;
using System;

namespace SMWP.Level.Player;

public partial class PlayerMediator : Node {
    [Export] public CharacterBody2D player { get; private set; } = null!;
    [Export] public PlayerMovement playerMovement { get; private set; } = null!;
    [Export] public PlayerDieAndHurt playerDieAndHurt { get; private set; } = null!;
    [Export] public PlayerAnimation playerAnimation { get; private set; } = null!;
    [Export] public PlayerSuit playerSuit { get; private set; } = null!;

    public override void _Ready() {
        player.SetMeta("PlayerMovement", playerMovement);
        player.SetMeta("PlayerDieAndHurt", playerDieAndHurt);
        player.SetMeta("PlayerAnimation", playerAnimation);
        player.SetMeta("PlayerSuit", playerSuit);
    }
}
