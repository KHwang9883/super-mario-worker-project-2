using Godot;
using System;
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
            var interactionWithBeetrootNode = result.GetNodeOrNull<Node>("InteractionWithBeetroot");
            if (interactionWithBeetrootNode is IBeetrootHittable beetrootHittable) {
                Explode();
                // 因为是帧伤所以不能用信号
                if (beetrootHittable.OnBeetrootHit(_beetroot)) {
                    _beetrootMovement.Bounce();
                } else {
                    _beetrootMovement.BounceCountAdd();
                }
            }
        }
        
        // Todo: 撞击砖块
        //
    }
    
    // 甜菜爆炸！
    public void Explode() {
        var fireballExplosion = _fireballExplosionPackedScene.Instantiate<Node2D>();
        fireballExplosion.Position = _beetroot.Position;
        _beetroot.AddSibling(fireballExplosion);
    }
}
