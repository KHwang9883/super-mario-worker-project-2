using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaFireballInteraction : Node, IFireballHittable {
    [Signal]
    public delegate void FireballHitEventHandler(Node2D ancestor, Vector2 position);
    
    public void OnFireballHit(Node2D fireball) {
        var ancestor = GetParent<Node2D>();
        EmitSignal(SignalName.FireballHit, ancestor, ancestor.Position);
        ancestor.QueueFree();
        fireball.QueueFree();
    }
}
