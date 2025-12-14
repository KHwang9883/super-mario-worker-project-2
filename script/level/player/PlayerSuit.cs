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
    
    [Export] private ComboComponent _starmanCombo = null!;

    [Export] private SmwpPointLight2D _smwpPointLight2D = null!;
    
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
        // 无敌星状态光源半径变化
        _smwpPointLight2D.LightRadius = Mathf.MoveToward(_smwpPointLight2D.LightRadius, Starman ? 35f : 1f, 0.6f);

        if (!Starman) return;
        
        // 传送状态下无敌星计时暂停
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        
        StarmanTimer++;
        if (StarmanTimer < StarmanTime) return;
        StarmanOver();
    }

    public void StarmanOver() {
        Starman = false;
        StarmanTimer = 0;
        _starmanCombo.ResetCombo();
        EmitSignal(SignalName.PlayerStarmanFinished);
    }
}
