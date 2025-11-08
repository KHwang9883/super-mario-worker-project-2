using Godot;
using System;
using SMWP.Level.Bonus.Mushroom;
using SMWP.Level.Enemy;
using SMWP.Level.Interface;

public partial class MushroomBumpInteraction : InteractionWithBump {
    [Export] private MushroomMovement _mushroomMovement = null!;
    [Export] private float _bumpSpeedY = -7.5f;
    
    public override void OnTopped() {
        base.OnTopped();
        _mushroomMovement.SpeedY = _bumpSpeedY;
    }
}
