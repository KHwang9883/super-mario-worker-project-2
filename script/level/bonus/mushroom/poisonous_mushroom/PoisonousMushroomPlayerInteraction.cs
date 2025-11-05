using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Bonus.Mushroom.PoisonousMushroom;

public partial class PoisonousMushroomPlayerInteraction : Node, IHurtableAndKillable {
    public IHurtableAndKillable.HurtEnum HurtType => IHurtableAndKillable.HurtEnum.Die;
}
