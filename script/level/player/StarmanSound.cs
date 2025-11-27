using Godot;
using System;
using SMWP.Level;

public partial class StarmanSound : AudioStreamPlayer2D {
    public override void _PhysicsProcess(double delta) {
        if (LevelManager.IsLevelPass) Stop();
    }
}
