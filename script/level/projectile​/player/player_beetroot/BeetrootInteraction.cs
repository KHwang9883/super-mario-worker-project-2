using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.Beetroot;

public partial class BeetrootInteraction : Node {
    [Signal]
    public delegate void BeetrootBounceEventHandler();
    [Signal]
    public delegate void BeetrootBounceCountAddEventHandler();
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
            if (interactionWithBeetrootNode is IBeetrootHittable beetrootHittable) {
                Explode();
                // 因为是帧伤所以不用信号
                if (beetrootHittable.OnBeetrootHit(_beetroot)) {
                    _beetrootMovement.Bounce();
                } else {
                    _beetrootMovement.BounceCountAdd();
                }
                // 并且只能作用于一个对象
                break;
            }
        }
        
        // 撞击砖块
        Node? interactionWithBlockNode = null;
        
        var blockCollider = _beetroot.MoveAndCollide(new Vector2(1f * _beetrootMovement.Direction, 1f), true)?.GetCollider();
        //GD.Print(_blockCollider);
        if (blockCollider is StaticBody2D staticBody2D) {
            if (staticBody2D.HasMeta("InteractionWithBlock")) {
                interactionWithBlockNode = (Node)staticBody2D.GetMeta("InteractionWithBlock");
            }
            if (interactionWithBlockNode is BlockHit blockHit) {
                blockHit.OnBlockHit(_beetroot);
                _beetrootMovement.Bounce();
            }
        }
    }
    
    // 甜菜爆炸！
    public void Explode() {
        var fireballExplosion = _fireballExplosionPackedScene.Instantiate<Node2D>();
        fireballExplosion.Position = _beetroot.Position;
        _beetroot.AddSibling(fireballExplosion);
    }
}
