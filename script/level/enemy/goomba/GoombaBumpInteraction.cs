using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaBumpInteraction : Node, IToppable {
    [Signal]
    public delegate void ToppedEventHandler();
    
    public void OnTopped() {
        EmitSignal(SignalName.Topped);
    }
}
