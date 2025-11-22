using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

[GlobalClass]
public partial class BackgroundParallax : Parallax2D {
    [Export] private float _parallaxCoefficient = 0.1f;
    private Node2D? _player;
    private float _playerLastPositionX;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _playerLastPositionX = _player.Position.X;
        _levelConfig = (LevelConfig)GetTree().GetFirstNodeInGroup("level_config");
    }
    public override void _PhysicsProcess(double delta) {
        // 视差滚动
        var screen = ScreenUtils.GetScreenRect(this);
        if ((_player == null)
            || (_levelConfig == null)
            || !(Math.Abs(_playerLastPositionX - _player.Position.X) > 0.1f)
            || !(_player.Position.X > screen.Size.X / 2)
            || !(_player.Position.X < _levelConfig.RoomWidth - screen.Size.X / 2)) return;
        
        var deltaX = _player.Position.X - _playerLastPositionX;
        _playerLastPositionX = _player.Position.X;
        var parallaxCoefficient = deltaX * _parallaxCoefficient;
        ScrollOffset -= new Vector2(parallaxCoefficient, 0f);
    }
}
