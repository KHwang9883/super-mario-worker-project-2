using Godot;
using System;
using SMWP.Level.Player;
using SMWP.Level.Projectile.Player.PlayerFireball;

namespace SMWP.Level.Player;

public partial class PlayerShoot : Node {
    [Signal]
    public delegate void PlayerShootingEventHandler();
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private PackedScene _fireballScene = null!;
    [Export] private PackedScene _beetrootScene = null!;
    
    public override void _PhysicsProcess(double delta) {
        if (Input.IsActionJustPressed("move_fire") && /*_playerMediator.playerMovement.Fire &&*/
            _playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Powered) {
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
                    EmitSignal(SignalName.PlayerShooting);
                    break;
                case PlayerSuit.PowerupEnum.Beetroot:
                    if (GetTree().GetNodesInGroup("beetroot").Count >= 2) break;
                    
                    var beetrootInstance = _beetrootScene.Instantiate<CharacterBody2D>();
                    // movement component
                    beetrootInstance.Position = new Vector2(
                        _playerMediator.player.Position.X + _playerMediator.playerMovement.Direction * 10f,
                        _playerMediator.player.Position.Y - 36f
                        );
                        // also
                    _playerMediator.player.AddSibling(beetrootInstance);
                    _playerMediator.playerAnimation.Fire = true;
                    EmitSignal(SignalName.PlayerShooting);
                    break;
            }
        }
    }
}
