using Godot;
using System;
using SMWP.Interface;

public partial class PoisonousMushroomPlayerInteraction : Node, IHurtableAndKillable {
    public IHurtableAndKillable.HurtEnum HurtType => IHurtableAndKillable.HurtEnum.Die;
}
