using Godot;
using System;
using SMWP.Interface;

public partial class GoombaFireballInteraction : Node, IFireballHittable {
    [Signal]
    public delegate void CreateDeadEventHandler(Node2D ancestor, Vector2 position);
    
    public void OnFireballHit(Node2D fireball) {
        var ancestor = GetParent<Node2D>();
        EmitSignal(SignalName.CreateDead, ancestor, ancestor.Position);
        ancestor.QueueFree();
        fireball.QueueFree();
    }
}
