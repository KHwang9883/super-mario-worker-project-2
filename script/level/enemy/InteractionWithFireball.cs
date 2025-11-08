using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class InteractionWithFireball : Node, IFireballHittable {
    [Signal]
    public delegate void FireballHitEventHandler();
    [Export] public bool FireballExplode { get; set; } = true;

    public virtual bool OnFireballHit(Node2D fireball) {
        EmitSignal(SignalName.FireballHit);
        return FireballExplode;
    }
}
