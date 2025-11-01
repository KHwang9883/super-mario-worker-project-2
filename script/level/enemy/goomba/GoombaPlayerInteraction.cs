using Godot;
using System;
using SMWP.Level.Enemy;

public partial class GoombaPlayerInteraction : Node, IStompable, IHurtableAndKillable {
    [Signal]
    public delegate void OnStompedEventHandler(Node2D stomper);
    
    private Node _ancestor = null!;

    public override void _Ready() {
        _ancestor = GetParent();
    }
    public void Stomped(Node2D stomper) {
        EmitSignal(SignalName.OnStomped, stomper);
        _ancestor.QueueFree();
    }
}
