using Godot;
using System;

[GlobalClass]
public partial class BackgroundParallax : Parallax2D {
    [Export] private float _parallaxCoefficient = 0.1f;
    private Node2D? _player;
    private float _playerLastPositionX;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _playerLastPositionX = _player.Position.X;
    }
    public override void _PhysicsProcess(double delta) {
        // 视差滚动
        if ((_player == null)
            || !(Math.Abs(_playerLastPositionX - _player.Position.X) > 0.1f)) return;
        var deltaX = _player.Position.X - _playerLastPositionX;
        _playerLastPositionX = _player.Position.X;
        var parallaxCoefficient = deltaX * _parallaxCoefficient;
        ScrollOffset -= new Vector2(parallaxCoefficient, 0f);
    }
}
