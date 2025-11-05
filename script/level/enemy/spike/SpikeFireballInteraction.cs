using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy.Spike;

public partial class SpikeFireballInteraction : Node, IFireballHittable
{
    public void OnFireballHit(Node2D fireball) {
        fireball.QueueFree();
    }
}
