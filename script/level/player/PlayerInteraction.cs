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

    [Export] private ComboComponent? _starmanCombo;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        // Todo: 水管传送的状态下不会受到伤害，这里暂时用卡墙的状态替代
        if (_playerMediator.playerMovement.Stuck) return;
        
        foreach (var result in results) {
            // 无敌星状态下击杀敌人（比踩踏更优先检测）
            StarmanAttackDetect(result);

            // 踩踏可踩踏物件
            StompAttackDetect(result);

            // 有伤害物件，不可踩或者踩踏失败
            HurtableAndKillableDetect(result);
        }

        // 顶砖检测（考虑到游戏特性，不采用foreach判断）
        HitBlockDetect();
    }

    public void StarmanAttackDetect(Node2D result) {
        Node? interactionWithStarNode = null;
        
        if (result.HasMeta("InteractionWithStar")) {
            interactionWithStarNode = (Node)result.GetMeta("InteractionWithStar");
        }
        if (!_playerMediator.playerSuit.Starman) return;
        if (interactionWithStarNode is not IStarHittable starHittable) return;
        if (_starmanCombo != null) {
            starHittable.OnStarmanHit(_starmanCombo.AddCombo());
        }
    }
    public void StompAttackDetect(Node2D result) {
        Node? interactionWithStompNode = null;
        
        if (result.HasMeta("InteractionWithStomp")) {
            interactionWithStompNode = (Node)result.GetMeta("InteractionWithStomp");
        }
        if (interactionWithStompNode == null) return;
        if (interactionWithStompNode is not IStompable stompable) return;
        if (stompable.Stompable
            && _player.Velocity.Y > 0f
            && _player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset) {
            EmitSignal(SignalName.PlayerStomp, stompable.OnStomped(_player));
        }
    }
    public void HurtableAndKillableDetect(Node2D result) {
        Node? interactionWithHurtNode = null;
        
        if (result.HasMeta("InteractionWithHurt")) {
            interactionWithHurtNode = (Node)result.GetMeta("InteractionWithHurt");
        }
        if (interactionWithHurtNode is not IHurtableAndKillable hurtableAndKillable) return;
        if (hurtableAndKillable is IStompable stompableAndHurtable) {
            if (stompableAndHurtable.Stompable) {
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
    public void BonusItemDetect(Node result) {
        // 奖励物
        Node? powerupSetNode = null;
                
        if (result.HasMeta("PowerupSet")) {
            powerupSetNode = (Node)result.GetMeta("PowerupSet");
        }
        if (powerupSetNode is not PowerupSet powerupSet) return;
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
    public void HitBlockDetect() {
        Node? blockHitNode = null;

        if (!(_playerMediator.playerMovement.SpeedY <= 0f)) return;
        var blockCollider = _player.MoveAndCollide(new Vector2(0f, -1f), true)?.GetCollider();
        //GD.Print(_blockCollider);
        if (blockCollider is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            blockHitNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (blockHitNode is not BlockHit blockHit) return;
        blockHit.OnBlockHit(_player);
        _playerMediator.playerMovement.SpeedY = 0f;
    }
}
