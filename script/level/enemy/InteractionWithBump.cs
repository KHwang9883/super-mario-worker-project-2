using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

[GlobalClass]
public partial class InteractionWithBump : Node, IToppable {
    [Signal]
    public delegate void ToppedEventHandler();
    
    public void OnTopped() {
        EmitSignal(SignalName.Topped);
    }
}
