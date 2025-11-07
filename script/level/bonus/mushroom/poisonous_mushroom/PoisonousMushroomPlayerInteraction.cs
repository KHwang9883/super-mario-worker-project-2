using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Bonus.Mushroom.PoisonousMushroom;

public partial class PoisonousMushroomPlayerInteraction : Node, IHurtableAndKillable {
    [Export] public IHurtableAndKillable.HurtEnum HurtType { get; set; }
}
