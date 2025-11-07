using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Player;
using SMWP.Level.Block.Brick;
using SMWP.Level.Bonus;
using SMWP.Level.Interface;

namespace SMWP.Level.Player;

public partial class PlayerInteraction : Node
{
    [Signal]
    public delegate void PlayerDieEventHandler();
    [Signal]
    public delegate void PlayerHurtProcessEventHandler();
    [Signal]
    public delegate void PlayerStompEventHandler(float stompSpeedY);
    [Signal]
    public delegate void PlayerPowerupEventHandler();
    [Signal]
    public delegate void PlayerPowerPlainEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    
    private GodotObject? _blockCollider = null!;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        foreach (var result in results) {
            // 踩踏可踩踏物件
            var interactionWithPlayerNode = result.GetNodeOrNull<Node>("InteractionWithPlayer");
            if (interactionWithPlayerNode != null) {
                if (interactionWithPlayerNode is IStompable stompable) {
                    if (_player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset &&
                        _player.Velocity.Y > 0f) {
                        //stompable.OnStomped(_player);
                        EmitSignal(SignalName.PlayerStomp, stompable.OnStomped(_player));
                    }
                }
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
                                EmitSignal(SignalName.PlayerHurtProcess);
                                break;
                        }
                    }
                } else {
                    switch (hurtableAndKillable.HurtType) {
                        case IHurtableAndKillable.HurtEnum.Die:
                            EmitSignal(SignalName.PlayerDie);
                            break;
                        case IHurtableAndKillable.HurtEnum.Hurt:
                            EmitSignal(SignalName.PlayerHurtProcess);
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
                    if (powerupSetNode.PowerupType != PowerupSet.PowerupEnum.Mushroom) {
                        _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Powered;
                    }
                }
                if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Small) {
                    _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Super;
                }
                if (_playerMediator.playerSuit.Suit != originalSuit 
                    || _playerMediator.playerSuit.Powerup != originalPowerup) {
                    EmitSignal(SignalName.PlayerPowerup);
                } else if (_playerMediator.playerSuit.Powerup == originalPowerup
                    && _playerMediator.playerSuit.Suit == originalSuit) {
                    EmitSignal(SignalName.PlayerPowerPlain);
                }
            }
        }
        
        // 顶砖检测
        if (_playerMediator.playerMovement.SpeedY <= 0f) {
            _blockCollider = _player.MoveAndCollide(new Vector2(0f, -1f), true)?.GetCollider();
            //GD.Print(_blockCollider);
            if (_blockCollider is StaticBody2D staticBody2D) {
                if (staticBody2D.GetNodeOrNull<BlockHit>("BlockHit") is BlockHit blockHit) {
                    blockHit.OnBlockHit(_player);
                    _playerMediator.playerMovement.SpeedY = 0f;
                }
            }
        }
    }
}
