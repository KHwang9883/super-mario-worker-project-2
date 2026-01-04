using Godot;
using System;

public partial class KickEffect : Node2D {
    public override void _Ready() {
        var player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        var sprite = GetNode<Sprite2D>("KickEffect");
        if (Position.X < player.Position.X) {
            sprite.FlipH = true;
        }
        var aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        aniPlayer.AnimationFinished += Destroy;
    }

    public void Destroy(StringName finishedAnimation) {
        QueueFree();
    }
}
