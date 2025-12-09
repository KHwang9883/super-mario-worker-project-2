using Godot;
using System;
using SMWP.Level.Physics;

public partial class SuperStarMovement : BasicMovement {
    [Export] private BonusSprout _bonusSprout = null!;
    
    public override void _PhysicsProcess(double delta) {
        if (_bonusSprout.Overlapping) return;
        
        base._PhysicsProcess(delta);
    }
}
