using Godot;
using System;
using SMWP.Level.Enemy;

public partial class FireInteractionWithTail : InteractionWithTail {
    [Export] private PackedScene _smokeScene = GD.Load<PackedScene>("uid://c707h2fhiiirw");
    [Export] private bool _smallerSmoke = true;

    public override bool OnRaccoonTailHit(Node2D tail) {
        var parent = GetParent<Node2D>();
        var smoke = _smokeScene.Instantiate<Node2D>();
        smoke.Position = parent.Position;
        if (_smallerSmoke) {
            smoke.Scale *= 0.5f;
        }
        parent.AddSibling(smoke);
        Callable.From(() => {
            parent.QueueFree();
        }).CallDeferred();
        return base.OnRaccoonTailHit(tail);
    }
}
