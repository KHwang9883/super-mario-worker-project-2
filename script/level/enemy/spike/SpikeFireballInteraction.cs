using Godot;
using System;
using SMWP.Interface;

public partial class SpikeFireballInteraction : Node, IFireballHittable
{
    public void OnFireballHit(Node2D fireball) {
        fireball.QueueFree();
    }
}
