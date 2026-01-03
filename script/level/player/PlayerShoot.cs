using Godot;
using System;
using System.Runtime.InteropServices.ComTypes;
using SMWP.Level.Player;
using SMWP.Level.Projectile.Player.Beetroot;
using SMWP.Level.Projectile.Player.PlayerFireball;

namespace SMWP.Level.Player;

public partial class PlayerShoot : Node {
    [Signal]
    public delegate void PlaySoundShootEventHandler();
    [Signal]
    public delegate void PlaySoundSpinEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private PackedScene _fireballScene = null!;
    [Export] private PackedScene _beetrootScene = null!;
    [Export] private PackedScene _raccoonTailScene = null!;
    
    public override void _PhysicsProcess(double delta) {
        if (_playerMediator.playerGodMode.IsGodFly || _playerMediator.playerMovement.IsInPipeTransport) return;

        if (!Input.IsActionJustPressed("move_fire")) {
            return;
        }
        if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Powered) {
            _playerMediator.playerAnimation.Fire = false;
            _playerMediator.playerAnimation.RaccoonFlyingUp = false;
            return;
        }
        switch (_playerMediator.playerSuit.Powerup) {
            case PlayerSuit.PowerupEnum.Fireball:
                if (GetTree().GetNodesInGroup("fireball").Count >= 2) break;
                    
                var fireballInstance = _fireballScene.Instantiate<CharacterBody2D>();
                var fireballMovement = fireballInstance.GetNode<FireballMovement>("FireballMovement");
                fireballInstance.Position = new Vector2(
                    _playerMediator.player.Position.X + _playerMediator.playerMovement.Direction * 10f,
                    _playerMediator.player.Position.Y - 24f
                );
                fireballMovement.Direction = _playerMediator.playerMovement.Direction;
                _playerMediator.player.AddSibling(fireballInstance);
                _playerMediator.playerAnimation.Fire = true;
                EmitSignal(SignalName.PlaySoundShoot);
                break;
            
            case PlayerSuit.PowerupEnum.Beetroot:
                if (GetTree().GetNodesInGroup("beetroot").Count >= 2) break;
                    
                var beetrootInstance = _beetrootScene.Instantiate<CharacterBody2D>();
                var beetrootMovement = beetrootInstance.GetNode<BeetrootMovement>("BeetrootMovement");
                beetrootInstance.Position = new Vector2(
                    _playerMediator.player.Position.X + _playerMediator.playerMovement.Direction * 10f,
                    _playerMediator.player.Position.Y - 36f
                );
                beetrootMovement.Direction = _playerMediator.playerMovement.Direction;
                _playerMediator.player.AddSibling(beetrootInstance);
                _playerMediator.playerAnimation.Fire = true;
                EmitSignal(SignalName.PlaySoundShoot);
                break;
            
            case PlayerSuit.PowerupEnum.Raccoon:
                if (GetTree().GetNodesInGroup("tail").Count > 0
                    || _playerMediator.playerMovement.Crouched) break;
                
                var tail = _raccoonTailScene.Instantiate<CharacterBody2D>();
                tail.Position = Vector2.Zero;
                _playerMediator.player.AddChild(tail);
                _playerMediator.playerAnimation.Fire = true;
                EmitSignal(SignalName.PlaySoundSpin);
                break;
        }
    }
}
