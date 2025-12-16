using Godot;
using System;
using SMWP;
using SMWP.Level;

public partial class Coin : Area2D {
    [Signal]
    public delegate void PlaySoundCoinEventHandler();
    
    public void OnBodyEntered(Node2D body) {
        GameManager.AddCoin(1);
        EmitSignal(SignalName.PlaySoundCoin);
        QueueFree();
    }
}
