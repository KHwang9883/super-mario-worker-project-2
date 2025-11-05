using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

namespace SMWP.Level.Projectile.Player.PlayerFireball;

public partial class FireballInteraction : Node {
    [Export] private CharacterBody2D _fireball = null!;
    [Export] private PackedScene _fireballExplosionPackedScene = null!;
    
    public override void _PhysicsProcess(double delta) {
        var results = ShapeQueryResult.ShapeQuery(_fireball, _fireball.GetNode<ShapeCast2D>("AreaBodyCollision"));

        foreach (var result in results) {
            // 检测可以被火球击中的物件
            var interactionWithFireballNode = result.GetNodeOrNull<Node>("InteractionWithFireball");
            if (interactionWithFireballNode is IFireballHittable fireballHittable) {
                fireballHittable.OnFireballHit(_fireball);
                Explode();
            }
        }
    }
    
    // 火球爆炸！
    public void Explode() {
        var fireballExplosion = _fireballExplosionPackedScene.Instantiate<Node2D>();
        fireballExplosion.Position = _fireball.GlobalPosition;
        _fireball.AddSibling(fireballExplosion);
    }
}
