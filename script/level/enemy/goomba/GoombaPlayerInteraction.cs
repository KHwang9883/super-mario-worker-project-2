using Godot;
using System;
using SMWP.Interface;

public partial class GoombaPlayerInteraction : Node, IStompable, IHurtableAndKillable {
    [Signal]
    public delegate void OnStompedEventHandler(Node2D stomper);
    [Signal]
    public delegate void CreateDeadEventHandler(Vector2 position);
    
    private Node2D _ancestor = null!;

    public override void _Ready() {
        _ancestor = GetParent<Node2D>();
    }
    public void Stomped(Node2D stomper) {
        EmitSignal(SignalName.OnStomped, stomper);
        EmitSignal(SignalName.CreateDead, _ancestor.Position);
        _ancestor.QueueFree();
    }
}
