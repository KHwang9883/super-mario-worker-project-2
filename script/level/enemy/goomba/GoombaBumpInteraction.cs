using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaBumpInteraction : Node, IToppable {
    [Signal]
    public delegate void ToppedEventHandler(Node2D ancestor, Vector2 position);
    
    public void OnTopped() {
        var ancestor = GetParent<Node2D>();
        EmitSignal(SignalName.Topped, ancestor, ancestor.Position);
        ancestor.QueueFree();
    }
}
