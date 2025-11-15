using Godot;
using System;
using SMWP.Level.Tool;

public partial class BulletBillCannonMovement : Node {
    [Signal]
    public delegate void PlaySoundBulletEventHandler();

    [Export] private float _triggerDistance = 75f;
    [Export] private PackedScene _bulletBillScene = GD.Load<PackedScene>("uid://btyhhhdhjf3sx");
    [Export] private float _shootTime = 200f;
    [Export] private float _shootTimeBonus = 0.1f;
    private Node2D? _parent;
    private Node2D? _player;
    private float _timer;
    private float _timeAddSpeedImproved;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready() {
        _parent ??= GetParent<Node2D>();
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _timeAddSpeedImproved = 1f + _rng.RandfRange(0f, _shootTimeBonus);
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null || _player == null) return;
        if ((_player.Position.X < _parent.Position.X + _triggerDistance
            && _player.Position.X > _parent.Position.X - _triggerDistance)
            || (_parent.Position.X < ScreenUtils.GetScreenRect(this).Position.X - 80f)
            || (_parent.Position.X > ScreenUtils.GetScreenRect(this).End.X + 80f)
            || (_parent.Position.Y < ScreenUtils.GetScreenRect(this).Position.Y - 16f)
            || (_parent.Position.Y > ScreenUtils.GetScreenRect(this).End.Y + 16f)
            ) return;
        _timer += _timeAddSpeedImproved;
        if (_timer < _shootTime) return;
        Launch();
    }
    public void Launch() {
        if (_parent == null) return;
        var bulletBill = (Node2D)_bulletBillScene.Instantiate();
        bulletBill.Position = _parent.Position;
        _parent.AddSibling(bulletBill);
        _timer = 0f;
        _timeAddSpeedImproved = 1f + _rng.RandfRange(0f, _shootTimeBonus);
        
        // 一定纵向范围内播放音效
        if (_parent.Position.Y > ScreenUtils.GetScreenRect(this).Position.Y - 16f
            && _parent.Position.Y < ScreenUtils.GetScreenRect(this).End.Y + 16f) {
            EmitSignal(SignalName.PlaySoundBullet);
        }
    }
}
