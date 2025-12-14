using Godot;
using System;

public partial class KoopaScroll : Node2D {
    [Export] public float ScrollTriggerDistance = 320f;
    [Export] public float Speed = 1f;
    public float ScrollPosX;
    public int Id;

    public override void _Ready() {
        ScrollPosX = GlobalPosition.X;
    }
    /*public override void _PhysicsProcess(double delta) {
        if (IsInGroup("koopa_scroll")) {
            GD.Print($"{this} is in group.");
        }
    }*/

    public override void _ExitTree() {
        RemoveFromGroup("koopa_scroll");
    }
}
