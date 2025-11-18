using Godot;
using System;
using SMWP.Level;

public partial class MushroomLifeAddLife : Area2D {
    [Signal]
    public delegate void MushroomLifeCollectedEventHandler();
    
    public void OnBodyEntered(Node2D body) {
        EmitSignal(SignalName.MushroomLifeCollected);
        GetParent().QueueFree();
    }
}
