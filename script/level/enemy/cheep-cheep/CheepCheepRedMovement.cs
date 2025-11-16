using Godot;
using System;

public partial class CheepCheepRedMovement : CheepCheepMovement {
    private int _timer;
    private RandomNumberGenerator _rng = new();
    
    public override void SwimMove(double delta) {
        SpeedXProcess();
        
        SpeedYProcess();
        
        ApplySpeed();

        Move();
    }
    public new void SpeedYProcess() {
        // y 速度
        _timer++;
        if (_timer < 101) return;
        _timer = 0;
        var speedYDecision = (int)Math.Round(_rng.RandfRange(0f, 10f));
        switch (speedYDecision) {
            case < 5:
                SpeedY = 0.1f;
                break;
            case > 5:
                SpeedY = -0.1f;
                break;
            case 5:
                SpeedY = 0f;
                break;
        }
    }
}
