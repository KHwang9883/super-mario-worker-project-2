using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaPlayerInteraction : Node, IStompable, IHurtableAndKillable {
    [Signal]
    public delegate void OnStompedEventHandler(Node2D stomper);
    
    public void Stomped(Node2D stomper) {
        var ancestor = GetParent<Node2D>();
        EmitSignal(SignalName.OnStomped, ancestor, ancestor.Position);
        ancestor.QueueFree();
    }
}
