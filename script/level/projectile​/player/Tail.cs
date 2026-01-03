using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Player;

namespace SMWP.Level.Projectile.Player;

public partial class Tail: CharacterBody2D {
    [Export] private AnimationPlayer _tailAniPos = null!;
    private CharacterBody2D? _player;
    private PlayerMovement? _playerMovement;
    private PlayerSuit? _playerSuit;

    public override void _Ready() {
        _player = (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        if (_player.HasMeta("PlayerMovement")) {
            _playerMovement = (PlayerMovement)_player.GetMeta("PlayerMovement");
        }
        if (_player.HasMeta("PlayerSuit")) {
            _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
        }
        if (_playerSuit == null) {
            GD.PushError("Tail: _playerSuit is null!");
            return;
        }
        if (_playerMovement == null) {
            GD.PushError("Tail: _playerMovement is null!");
            return;
        }
        _tailAniPos.Play(_playerMovement.Direction > 0 ? "right" : "left");
    }

    public override void _PhysicsProcess(double delta) {
        // 下落扫尾可以避免踩踏
        if (_playerMovement!.SpeedY > 0f 
            && _player!.MoveAndCollide(
                new Vector2(0f, _playerMovement!.SpeedY + 1f), true, 0.02f
                ) == null
            ) {
            Position = Position with { Y = _playerMovement.SpeedY };
        } else {
            Position = Position with { Y = 0f };
        }

        // 不是浣熊装判定立刻销毁
        if (_playerSuit is not { Suit: PlayerSuit.SuitEnum.Powered, Powerup: PlayerSuit.PowerupEnum.Raccoon }) {
            QueueFree();
        }

        // 撞砖判定
        Node? interactionWithBlockNode = null;
        var blockCollider =
            MoveAndCollide(Vector2.Zero, true)?.GetCollider();
        //GD.Print(blockCollider);
        if (blockCollider is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        
        // 隐藏砖可以触发
        //if (blockHit.Hidden) return;

        blockHit.OnBlockHit(this);
    }
}
