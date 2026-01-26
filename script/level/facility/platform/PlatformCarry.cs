using Godot;
using System;

public partial class PlatformCarry : StaticBody2D {
    private PlatformHorizontal _platformHorizontal = null!;
    private uint _originCollisionLayer;
    
    public override void _Ready() {
        base._Ready();
        _originCollisionLayer = CollisionLayer;
        CollisionLayer = 0;
        _platformHorizontal = GetParent().GetParent<PlatformHorizontal>();
        _platformHorizontal.PlatformCarryOn += Enable;
        _platformHorizontal.PlatformCarryOff += Disable;
    }

    public void Enable() {
        CollisionLayer = _originCollisionLayer;
    }

    public void Disable() {
        CollisionLayer = 0;
    }
}
