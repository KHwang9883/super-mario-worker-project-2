using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Goomba;

public partial class GoombaFireballInteraction : Node, IFireballHittable {
    [Signal]
    public delegate void FireballHitEventHandler();
    
    public void OnFireballHit(Node2D fireball) {
        EmitSignal(SignalName.FireballHit);
        fireball.QueueFree();
    }
}
