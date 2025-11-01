using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Player;

public partial class PlayerInteraction : Node
{
    [Signal]
    public delegate void PlayerDieEventHandler();
    [Signal]
    public delegate void PlayerHurtEventHandler();
    [Signal]
    public delegate void PlayerStompEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;

    public override void _PhysicsProcess(double delta) {
        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        foreach (var result in results) {
            // 踩踏可踩踏物件
            var interactionNode = result.GetNodeOrNull<Node>("InteractionWithPlayer");
            if (interactionNode == null) {
                continue;
            }
            if (interactionNode is IStompable stompable) {
                if (_player.GlobalPosition.Y < result.GlobalPosition.Y + stompable.StompOffset && _player.Velocity.Y > 0f) {
                    stompable.Stomped(_player);
                    EmitSignal(SignalName.PlayerStomp);
                }
            }
            
            // 有伤害物件，不可踩或者踩踏失败
            if (interactionNode is IHurtableAndKillable hurtableAndKillable) {
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
        }
    }
}
