using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaPlayerInteraction : Node, IStompable, IHurtableAndKillable {
    [Signal]
    public delegate void StompedEventHandler();
    
    public void OnStomped(Node2D stomper) {
        var ancestor = GetParent<Node2D>();
        EmitSignal(SignalName.Stomped);
        ancestor.QueueFree();
    }
}
