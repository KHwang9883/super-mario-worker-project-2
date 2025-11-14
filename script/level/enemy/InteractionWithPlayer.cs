using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithPlayer : Node, IStompable, IHurtableAndKillable {
    [Signal]
    public delegate void StompedEventHandler();
    
    [Export] public IHurtableAndKillable.HurtEnum HurtType { get; set; }

    [Export] public bool Stompable { get; set; }
    [Export] public float StompOffset { get; set; } = -12f;
    [Export] public float StompSpeedY { get; set; } = -8f;
    protected Node2D? Parent;

    public override void _Ready() {
        Parent = (Node2D)GetParent();
        MetadataInject(Parent);
    }
    public void MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStomp", this);
        parent?.SetMeta("InteractionWithHurt", this);
    }
    public float OnStomped(Node2D stomper) {
        EmitSignal(SignalName.Stomped);
        return StompSpeedY;
    }
    public virtual void PlayerHurtCheck(bool check) {
    }
}
