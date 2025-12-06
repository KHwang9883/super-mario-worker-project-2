using Godot;
using System;
using SMWP.Level.Player;

namespace SMWP.Level.Player;

public partial class PlayerSuit : Node {
    [Signal]
    public delegate void PlayerStarmanStartedEventHandler();
    [Signal]
    public delegate void PlayerStarmanFinishedEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    
    public enum SuitEnum {
        Small,
        Super,
        Powered,
    }
    [Export] public SuitEnum Suit = SuitEnum.Small;
    public enum PowerupEnum {
        Fireball,
        Beetroot,
        Lui,
    }
    [Export] public PowerupEnum Powerup = PowerupEnum.Fireball;
    [Export] public bool Starman;
    [Export] public int StarmanTime { get; set; } = 500;
    public int StarmanTimer { get; set; }

    public void SetStarmanState() {
        StarmanTimer = 0;
        if (Starman) return;
        Starman = true;
        EmitSignal(SignalName.PlayerStarmanStarted);
    }
    public override void _PhysicsProcess(double delta) {
        if (!Starman) return;
        
        // 传送状态下无敌星计时暂停
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        
        StarmanTimer++;
        if (StarmanTimer < StarmanTime) return;
        Starman = false;
        StarmanTimer = 0;
        EmitSignal(SignalName.PlayerStarmanFinished);
    }
}
