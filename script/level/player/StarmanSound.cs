using Godot;
using System;
using SMWP;
using SMWP.Level;

public partial class StarmanSound : AudioStreamPlayer2D {
    public override void _PhysicsProcess(double delta) {
        if (GameManager.IsLevelPass) Stop();
    }
}
