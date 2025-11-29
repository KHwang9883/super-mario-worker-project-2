using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.Beetroot;

public partial class BeetrootInteraction : Node {
    [Export] private CharacterBody2D _beetroot = null!;
    [Export] private PackedScene _fireballExplosionPackedScene = null!;
    [Export] private BeetrootMovement _beetrootMovement = null!;
    
    public override void _PhysicsProcess(double delta) {
        var results = ShapeQueryResult.ShapeQuery(_beetroot, _beetroot.GetNode<ShapeCast2D>("AreaBodyCollision"));

        foreach (var result in results) {
            // 检测可以被甜菜击中的物件
            Node? interactionWithBeetrootNode = null;
            
            if (result.HasMeta("InteractionWithBeetroot")) {
                interactionWithBeetrootNode = (Node)result.GetMeta("InteractionWithBeetroot");
            }
            if (interactionWithBeetrootNode is not IBeetrootHittable beetrootHittable) continue;
            Explode();
            // 因为是帧伤所以不用信号
            result.SetMeta("InteractingObject", _beetroot);
            if (beetrootHittable.OnBeetrootHit(_beetroot)) {
                _beetrootMovement.Bounce();
            } else {
                _beetrootMovement.BounceCountAdd();
            }
            // 并且只能作用于一个对象
            break;
        }
        
        // 考虑到执行顺序，撞击砖块的检测为 Movement 组件中发射信号
    }
    
    // 甜菜爆炸！
    public void Explode() {
        var fireballExplosion = _fireballExplosionPackedScene.Instantiate<Node2D>();
        fireballExplosion.Position = _beetroot.Position;
        _beetroot.AddSibling(fireballExplosion);
    }
    public void BlockHitDetect() {
        Node? interactionWithBlockNode = null;
        
        var blockCollider = _beetroot.MoveAndCollide(
            new Vector2(1f * _beetrootMovement.Direction, 1f), true
            )?.GetCollider();
        //GD.Print(blockCollider);
        if (blockCollider is not StaticBody2D staticBody2D) return;
        if (staticBody2D.HasMeta("InteractionWithBlock")) {
            interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not BlockHit blockHit) return;
        blockHit.OnBlockHit(_beetroot);
        
        // 因为砖块本身是实心，所以不需要额外 Bounce 方法的调用
        //_beetrootMovement.Bounce();
    }
}
