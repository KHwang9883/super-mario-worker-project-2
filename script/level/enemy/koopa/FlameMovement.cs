using Godot;
using System;
using SMWP.Level.Physics;

public partial class FlameMovement : BasicMovement {
    private float _originPosY;
    private float _targetPosY;
    private RandomNumberGenerator _rng = new();
    
    public override void _Ready() {
        base._Ready();
        
        _originPosY = MoveObject.GlobalPosition.Y;
        if (MoveObject.HasMeta("FixedPositionY")) {
            _targetPosY = (float)MoveObject.GetMeta("FixedPositionY") - 12f - _rng.RandiRange(0, 2) * 32f;
        } else {
            _targetPosY = _originPosY + _rng.RandiRange(0, 2) * 32f;
        }
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        
        MoveObject.GlobalPosition = MoveObject.GlobalPosition with {
            Y = Mathf.MoveToward(MoveObject.GlobalPosition.Y, _targetPosY, 2f),
        };
    }
}
