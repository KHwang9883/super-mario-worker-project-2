using Godot;
using System;

public partial class FireballExplode : Node {
    [Export] private PackedScene _fireballExplosionPackedScene = GD.Load<PackedScene>("uid://5mmyew6mh71p");
    private Node2D? _fireball;

    public override void _Ready() {
        _fireball ??= (Node2D)GetParent();
    }
    // 火球爆炸！
    public void OnFireballExplode() {
        if (_fireball == null) return;
        
        var fireballExplosion = _fireballExplosionPackedScene.Instantiate<Node2D>();
        fireballExplosion.Position = _fireball.GlobalPosition;
        _fireball.AddSibling(fireballExplosion);
        _fireball.QueueFree();
    }
}
