using Godot;
using System;

public partial class KoopaHUD : Node2D {
    public bool Activate;
    private float _originPosY;

    public override void _Ready() {
        _originPosY = Position.Y;
        Position += Vector2.Up * 120;
        ResetPhysicsInterpolation();
    }
    public override void _PhysicsProcess(double delta) {
        if (Activate) {
            Position = Position with { Y = Mathf.MoveToward(Position.Y, _originPosY, 1f) };
        }
        
        // Todo: 血量显示
    }
}
