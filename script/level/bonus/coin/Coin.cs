using Godot;
using System;
using SMWP.Level;

public partial class Coin : Area2D {
    [Signal]
    public delegate void PlaySoundCoinEventHandler();
    
    public void OnBodyEntered(Node2D body) {
        LevelManager.AddCoin(1);
        EmitSignal(SignalName.PlaySoundCoin);
        QueueFree();
    }
}
