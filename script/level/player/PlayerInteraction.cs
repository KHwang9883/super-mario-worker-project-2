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
    
    private int _starmanCombo;
    private int _starmanScore;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        
        foreach (var result in results) {
            Node? interactionWithHurtNode = null;
            Node? interactionWithStompNode = null;
            Node? interactionWithStarNode = null;
            Node? powerupSetNode = null;
            
            // Todo: 水管传送的状态下不会受到伤害，这里暂时用卡墙的状态替代
            
            if (!_playerMediator.playerMovement.Stuck) {
                if (result.HasMeta("InteractionWithHurt")) {
                    interactionWithHurtNode = (Node)result.GetMeta("InteractionWithHurt");
                }
                if (result.HasMeta("InteractionWithStomp")) {
                    interactionWithStompNode = (Node)result.GetMeta("InteractionWithStomp");
                }
                
                // 踩踏可踩踏物件
                if (interactionWithStompNode != null) {
                    if (interactionWithStompNode is IStompable stompable) {
                        if (stompable.Stompable && _player.Velocity.Y > 0f
                            && _player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset) {
                            EmitSignal(SignalName.PlayerStomp, stompable.OnStomped(_player));
                        }
                    }
                }

                // 有伤害物件，不可踩或者踩踏失败
                if (interactionWithHurtNode != null) {
                    if (interactionWithHurtNode is IHurtableAndKillable hurtableAndKillable) {
                        if (hurtableAndKillable is IStompable stompableAndHurtable) {
                            if (_player.GlobalPosition.Y >=
                                result.GlobalPosition.Y + stompableAndHurtable.StompOffset) {
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
                                case IHurtableAndKillable.HurtEnum.Nothing:
                                    break;
                            }
                        }
                    }
                }
            }
            
            // 无敌星状态下击杀敌人
            if (result.HasMeta("InteractionWithStar")) {
                interactionWithStarNode = (Node)result.GetMeta("InteractionWithStar");
            }
            if (_playerMediator.playerSuit.Starman) {
                if (interactionWithStarNode is IStarHittable starHittable) {
                    _starmanCombo++;
                    starHittable.OnStarmanHit(_starmanScore);
                    
                    // Todo: starman combo & score
                    
                } 
            }

            // 奖励物
            if (result.HasMeta("PowerupSet")) {
                powerupSetNode = (Node)result.GetMeta("PowerupSet");
            }
            if (powerupSetNode is PowerupSet powerupSet) {
                powerupSet.OnCollected();
                
                var originalSuit = _playerMediator.playerSuit.Suit;
                var originalPowerup = _playerMediator.playerSuit.Powerup;

                // 无敌星
                if (powerupSet.PowerupType == PowerupSet.PowerupEnum.SuperStar) {
                    _playerMediator.playerSuit.Starman = true;
                    _playerMediator.playerSuit.StarmanTimer = 0;
                } else {
                    // 常规补给
                    if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small) {
                        _playerMediator.playerSuit.Powerup = powerupSet.PowerupType switch {
                            PowerupSet.PowerupEnum.FireFlower => PlayerSuit.PowerupEnum.Fireball,
                            PowerupSet.PowerupEnum.Beetroot => PlayerSuit.PowerupEnum.Beetroot,
                            PowerupSet.PowerupEnum.Lui => PlayerSuit.PowerupEnum.Lui,
                            _ => _playerMediator.playerSuit.Powerup,
                        };
                        if (powerupSet.PowerupType != PowerupSet.PowerupEnum.Mushroom) {
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
        }
        
        // 顶砖检测
        Node? blockHitNode = null;
        
        if (_playerMediator.playerMovement.SpeedY <= 0f) {
            var blockCollider = _player.MoveAndCollide(new Vector2(0f, -1f), true)?.GetCollider();
            //GD.Print(_blockCollider);
            if (blockCollider is StaticBody2D staticBody2D) {
                if (staticBody2D.HasMeta("InteractionWithBlock")) {
                    blockHitNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
                }
                if (blockHitNode is BlockHit blockHit) {
                    blockHit.OnBlockHit(_player);
                    _playerMediator.playerMovement.SpeedY = 0f;
                }
            }
        }
    }
}
