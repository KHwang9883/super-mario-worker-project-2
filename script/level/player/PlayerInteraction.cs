using Godot;
using System;
using SMWP.Level.Player;
using SMWP.Interface;

public partial class PlayerInteraction : Node
{
    [Signal]
    public delegate void PlayerDieEventHandler();
    [Signal]
    public delegate void PlayerHurtEventHandler();
    [Signal]
    public delegate void PlayerStompEventHandler();
    [Signal]
    public delegate void PlayerPowerupEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        foreach (var result in results) {
            // 踩踏可踩踏物件
            var interactionWithPlayerNode = result.GetNodeOrNull<Node>("InteractionWithPlayer");
            if (interactionWithPlayerNode is IStompable stompable) {
                if (_player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset && _player.Velocity.Y > 0f) {
                    stompable.Stomped(_player);
                    EmitSignal(SignalName.PlayerStomp);
                }
                break;
            }

            // 有伤害物件，不可踩或者踩踏失败
            if (interactionWithPlayerNode is IHurtableAndKillable hurtableAndKillable) {
                if (hurtableAndKillable is IStompable stompableAndHurtable) {
                    if (_player.GlobalPosition.Y >= result.GlobalPosition.Y + stompableAndHurtable.StompOffset) {
                        switch (hurtableAndKillable.HurtType) {
                            case IHurtableAndKillable.HurtEnum.Die:
                                EmitSignal(SignalName.PlayerDie);
                                break;
                            case IHurtableAndKillable.HurtEnum.Hurt:
                                EmitSignal(SignalName.PlayerHurt);
                                break;
                        }
                    }
                } else {
                    switch (hurtableAndKillable.HurtType) {
                        case IHurtableAndKillable.HurtEnum.Die:
                            EmitSignal(SignalName.PlayerDie);
                            break;
                        case IHurtableAndKillable.HurtEnum.Hurt:
                            EmitSignal(SignalName.PlayerHurt);
                            break;
                    }
                }
            }
            
            // 奖励物
            var powerupSetNode = result.GetNodeOrNull<PowerupSet>("PowerupSet");
            if (powerupSetNode != null) {
                powerupSetNode.OnCollected();
                
                var originalSuit = _playerMediator.playerSuit.Suit;
                var originalPowerup = _playerMediator.playerSuit.Powerup;
                
                if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small) {
                    _playerMediator.playerSuit.Powerup = powerupSetNode.PowerupType switch {
                        PowerupSet.PowerupEnum.FireFlower => PlayerSuit.PowerupEnum.Fireball,
                        PowerupSet.PowerupEnum.Beetroot => PlayerSuit.PowerupEnum.Beetroot,
                        PowerupSet.PowerupEnum.Lui => PlayerSuit.PowerupEnum.Lui,
                        _ => _playerMediator.playerSuit.Powerup,
                    };
                    _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Powered;
                }
                if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Small) {
                    _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Super;
                }
                if (_playerMediator.playerSuit.Suit != originalSuit 
                    || _playerMediator.playerSuit.Powerup != originalPowerup) {
                    EmitSignal(SignalName.PlayerPowerup);
                }
            }
        }
    }
}
