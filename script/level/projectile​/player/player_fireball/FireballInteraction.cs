using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.PlayerFireball;

public partial class FireballInteraction : Node {
    [Signal]
    public delegate void FireballExplodeEventHandler();
    
    [Export] private CharacterBody2D? _fireball;
    
    public override void _PhysicsProcess(double delta) {
        if (_fireball == null) return;
        
        var results = ShapeQueryResult.ShapeQuery(_fireball, _fireball.GetNode<ShapeCast2D>("AreaBodyCollision"));

        foreach (var result in results) {
            // 检测可以被火球击中的物件
            Node? interactionWithFireballNode = null;
            
            if (result.HasMeta("InteractionWithFireball")) {
                interactionWithFireballNode = (Node)result.GetMeta("InteractionWithFireball");
            }
            if (interactionWithFireballNode is IFireballHittable fireballHittable) {
                result.SetMeta("InteractingObject", _fireball);
                if (fireballHittable.OnFireballHit(_fireball)) {
                    EmitSignal(SignalName.FireballExplode);
                }
                
                // 成功一次就结束本物理帧的遍历
                break;
            }
        }
        
        // Todo: 撞击冰块
    }
}
