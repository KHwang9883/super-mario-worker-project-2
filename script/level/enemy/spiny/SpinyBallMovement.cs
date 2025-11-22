using Godot;
using System;
using SMWP.Level.Physics;

public partial class SpinyBallMovement : BasicMovement {
    [Export] private PackedScene _spinyScene = GD.Load<PackedScene>("uid://lo64hov3eeo3");
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    
    private bool _initialOverlapWithWall;
    private bool _free;

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        _animatedSprite2D.RotationDegrees += 5f;
    }
    public override void Move() {
        // 刺球运动处理
        if (SpeedY > 0f && !_initialOverlapWithWall) {
            MoveObject.SetCollisionMask(5);
            if (MoveObject.MoveAndCollide(Vector2.Zero, true, 1f) != null) {
                _initialOverlapWithWall = true;
            } else {
                _free = true;
            }
        }

        MoveObject.SetCollisionMask(0);
        MoveObject.MoveAndSlide();

        if (_initialOverlapWithWall) {
            MoveObject.SetCollisionMask(5);
            if (MoveObject.MoveAndCollide(Vector2.Zero, true, 1f) == null) {
                _free = true;
            }
        }
        if (_free) {
            MoveObject.SetCollisionMask(5);
            if (MoveObject.MoveAndCollide(new Vector2(0f, SpeedY), true, 1f) != null) {
                Create();
            }
        }
    }
    public void Create() {
        var spiny = _spinyScene.Instantiate<Node2D>();
        spiny.Position = MoveObject.Position;
        MoveObject.AddSibling(spiny);
        MoveObject.QueueFree();
    }
}
