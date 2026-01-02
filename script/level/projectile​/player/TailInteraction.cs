using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player;

public partial class TailInteraction : Node {
    [Export] private CharacterBody2D _tail = null!;
    [Export] private ShapeCast2D _cast2D = null!;
    
    public override void _PhysicsProcess(double delta) {
        var results = ShapeQueryResult.ShapeQuery(_tail, _cast2D);

        foreach (var result in results) {
            // 检测可以被火球击中的物件
            Node? interactionWithTailNode = null;
            
            if (result.HasMeta("InteractionWithTail")) {
                interactionWithTailNode = (Node)result.GetMeta("InteractionWithTail");
            }
            if (interactionWithTailNode is IRaccoonTailHittable tailHittable) {
                result.SetMeta("InteractingObject", _tail);
                tailHittable.OnRaccoonTailHit(_tail);
                // 成功一次就结束本物理帧的遍历
                break;
            }
        }
    }
}
