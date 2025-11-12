using Godot;
using System;
using SMWP.Level.Physics;

public partial class FollowParentDebugLabel : Label {
    [Export] public CharacterBody2D Parent = null!;

    public override void _Ready() {
        Parent ??= (CharacterBody2D)GetParent();
    }
    public override void _PhysicsProcess(double delta) {
        var node = Parent.GetNode<BasicMovement>("BasicMovement");
        //Text = $"IT'S: {Parent.Velocity.X}";
        Text = $"{node.SpeedX}";
    }
}
