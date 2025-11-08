using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithBump : Node, IToppable {
    [Signal]
    public delegate void ToppedEventHandler();
    
    public virtual void OnTopped() {
        EmitSignal(SignalName.Topped);
    }
}
