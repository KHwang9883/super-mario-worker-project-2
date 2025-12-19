using Godot;
using System;
using SMWP.Level.Bonus.Mushroom;
using SMWP.Level.Enemy;
using SMWP.Level.Interface;

public partial class MushroomBumpInteraction : InteractionWithBump {
    [Export] private BonusSprout _bonusSprout = null!;
    
    [Export] private MushroomMovement _mushroomMovement = null!;
    [Export] private float _bumpSpeedY = -7.5f;
    
    public override void OnBumped() {
        if (!_bonusSprout.Detected) return;
        
        base.OnBumped();
        _mushroomMovement.SpeedY = _bumpSpeedY;
    }
}
