using Godot;
using System;

public partial class CheepCheepSpikeMovement : CheepCheepMovement {
    [Export] private Area2D? _outWaterDetect;
    private bool _headIsInWater;
    
    public override void SwimMove(double delta) {
        SpeedProcess();
        
        ApplySpeed();

        Move();
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        
        if (_outWaterDetect == null) return;
        
        _headIsInWater = false;
        foreach (var area in _outWaterDetect.GetOverlappingAreas()) {
            if (area.IsInGroup("water")) {
                _headIsInWater = true;
            }
        }
    }
    public void SpeedProcess() {
        if (Player == null || _outWaterDetect == null) return;
        
        if (MoveObject.Position.X < Player.Position.X) {
            SpeedX = 0.8f;
        } else if (MoveObject.Position.X > Player.Position.X) {
            SpeedX = -0.8f;
        }
        
        if (MoveObject.Position.Y < Player.Position.Y) {
            SpeedY = 0.8f;
        } else if (MoveObject.Position.Y > Player.Position.Y && _headIsInWater) {
            SpeedY = -0.8f;
        }
    }
}
