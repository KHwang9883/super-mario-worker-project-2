using Godot;
using System;
using Godot.Collections;
using SMWP.Level.Block;
using SMWP.Level.Player;
using SMWP.Level.Block.Brick;
using SMWP.Level.Bonus;
using SMWP.Level.Interface;

namespace SMWP.Level.Player;

public partial class PlayerInteraction : Node {
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
    
    [Export] private PlayerMediator? _playerMediator;
    [Export] private CharacterBody2D? _player;

    [Export] private ComboComponent? _starmanCombo;

    //private bool _isPlayerStomped;
    private bool _hurtFrame;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        if (_playerMediator == null) return;
        var results = _playerMediator.playerMovement.GetShapeQueryResults();

        _hurtFrame = false;
        
        // 水管传送的状态下不会进行交互检测
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        
        // 结果数组不为 null 时开始检测
        if (results == null) return;
        
        // 无敌星状态下击杀敌人（比踩踏更优先检测）
        StarmanAttackDetect(results);

        // 踩踏可踩踏物件
        StompAttackDetect(results);

        // 有伤害物件，不可踩或者踩踏失败
        HurtableAndKillableDetect(results);
            
        // 奖励物
        BonusItemDetect(results);

        // 顶砖检测（考虑到游戏特性，不采用foreach判断）
        HitBlockDetect();
    }

    public void StarmanAttackDetect(Array<Node2D> results) {
        if (_player == null || _playerMediator == null) return;
        
        foreach (var result in results) {
            Node? interactionWithStarNode = null;
            if (result == null) continue;
            
            if (result.HasMeta("InteractionWithStar")) {
                interactionWithStarNode = (Node)result.GetMeta("InteractionWithStar");
            }
            if (!_playerMediator.playerSuit.Starman) continue;
            if (interactionWithStarNode is not IStarHittable starHittable) continue;
            result.SetMeta("InteractingObject", _player);
            if (_starmanCombo == null || !starHittable.IsStarHittable) continue;
            // 成功一次就停止 foreach
            if (starHittable.OnStarmanHit(_starmanCombo.AddCombo())) break;
        }
    }
    public void StompAttackDetect(Array<Node2D> results) {
        if (_player == null) return;
        
        foreach (var result in results) {
            Node? interactionWithStompNode = null;
            if (result == null) continue;
            
            if (result.HasMeta("InteractionWithStomp")) {
                interactionWithStompNode = (Node)result.GetMeta("InteractionWithStomp");
            }
            if (interactionWithStompNode is not IStompable stompable) continue;
            if (!stompable.Stompable
                || !(_player.Velocity.Y > 0f)
                || !(_player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset)) continue;
            result.SetMeta("InteractingObject", _player);
            EmitSignal(SignalName.PlayerStomp, stompable.OnStomped(_player));
            // 成功一次就停止 foreach
            //_isPlayerStomped = true;
            break;
        }
    }
    public void HurtableAndKillableDetect(Array<Node2D> results) {
        if (_player == null) return;
        /*if (_isPlayerStomped) {
            _isPlayerStomped = false;
            return;
        }*/
        
        foreach (var result in results) {
            Node? interactionWithHurtNode = null;
            if (result == null) continue;

            if (result.HasMeta("InteractionWithHurt")) {
                interactionWithHurtNode = (Node)result.GetMeta("InteractionWithHurt");
            }
            if (interactionWithHurtNode is not IHurtableAndKillable hurtableAndKillable) continue;
            result.SetMeta("InteractingObject", _player);
            if (hurtableAndKillable is IStompable { Stompable: true } stompableAndHurtable) {
                if (!(_player.GlobalPosition.Y >=
                      result.GlobalPosition.Y + stompableAndHurtable.StompOffset)) {
                    // 此举针对静止龟壳
                    if (hurtableAndKillable.HurtType == IHurtableAndKillable.HurtEnum.Nothing) {
                        hurtableAndKillable.PlayerHurtCheck(_hurtFrame);
                    }
                    continue;
                }
            }
            switch (hurtableAndKillable.HurtType) {
                case IHurtableAndKillable.HurtEnum.Die:
                    EmitSignal(SignalName.PlayerDie);
                    break;
                case IHurtableAndKillable.HurtEnum.Hurt:
                    EmitSignal(SignalName.PlayerHurtProcess);
                    hurtableAndKillable.PlayerHurtCheck(_hurtFrame);
                    break;
                case IHurtableAndKillable.HurtEnum.Nothing:
                    hurtableAndKillable.PlayerHurtCheck(_hurtFrame);
                    break;
            }
        }
    }
    public void BonusItemDetect(Array<Node2D> results) {
        if (_playerMediator == null) return;
        
        foreach (var result in results) {
            Node? powerupSetNode = null;
            if (result == null) continue;

            if (result.HasMeta("PowerupSet")) {
                powerupSetNode = (Node)result.GetMeta("PowerupSet");
            }
            if (powerupSetNode is not PowerupSet powerupSet) continue;
            powerupSet.OnCollected();

            var originalSuit = _playerMediator.playerSuit.Suit;
            var originalPowerup = _playerMediator.playerSuit.Powerup;

            // 无敌星
            if (powerupSet.PowerupType == PowerupSet.PowerupEnum.SuperStar) {
                _playerMediator.playerSuit.SetStarmanState();
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
    public void HitBlockDetect() {
        if (_player == null || _playerMediator == null) return;
        
        Node? blockHitNode = null;

        if (!(_playerMediator.playerMovement.SpeedY <= 0f)) return;
        
        var collision = _player.MoveAndCollide(new Vector2(0f, -1f), true);
        if (collision == null) {
            return;
        }
        
        var blockCollider = collision.GetCollider();
        //GD.Print(blockCollider);
        if (blockCollider is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            blockHitNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (blockHitNode is not BlockHit blockHit) return;
        blockHit.OnBlockHit(_player);
        _playerMediator.playerMovement.SpeedY = 0f;
    }
    public void HurtSucceedSet() {
        _hurtFrame = true;
    }
}
