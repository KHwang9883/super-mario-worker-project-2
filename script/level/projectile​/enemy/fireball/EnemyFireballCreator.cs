using Godot;
using System;
using SMWP.Level.Tool;

namespace SMWP.Level.Projectile.Enemy;

public partial class EnemyFireballCreator : Node2D {
    [Signal]
    public delegate void PlaySoundFireballEventHandler();
    
    [Export] private PackedScene _enemyFireballScene = GD.Load<PackedScene>("uid://moorst8boiav");
    [Export] private int _bulletNumber = 3;
    [Export] private int _shootTime = 15;
    private int _timer;
    private int _bulletCount;
    private Node2D? _parent;
    
    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
    }
    public override void _PhysicsProcess(double delta) {
        if (_bulletCount >= _bulletNumber) QueueFree();
        else {
            Rect2 screenRect = ScreenUtils.GetScreenRect(this);
            if (Position.X < screenRect.Position.X - 100f
                || Position.X > screenRect.End.X + 100f
                || Position.Y < screenRect.Position.Y - 100f
                || Position.Y > screenRect.End.Y + 100f)
                return;
            _timer++;
            if (_timer < _shootTime) return;
            _timer = 0;
            Create();
        }
    }
    public void Create() {
        if (_parent == null) return;
        _bulletCount++;
        var enemyFireball = _enemyFireballScene.Instantiate<Node2D>();
        enemyFireball.Position =
            _parent.Position + Vector2.FromAngle(Mathf.DegToRad(_parent.RotationDegrees - 90f)) * 8f;
        enemyFireball.SetMeta("PiranhaPlantAngle", _parent.Rotation);
        _parent.AddSibling(enemyFireball);
        EmitSignal(SignalName.PlaySoundFireball);
    }
}
