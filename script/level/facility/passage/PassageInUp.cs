using Godot;
using System;
using SMWP.Level.Player;

public partial class PassageInUp : Area2D {
    [Export] private CollisionShape2D _collisionShape2D = null!;
    private RectangleShape2D _shapeSmall = GD.Load<RectangleShape2D>("uid://cn171rrcmnt0j");
    private RectangleShape2D _shapeNormal = GD.Load<RectangleShape2D>("uid://catao11iwfehm");
    
    private Node2D? _player;
    private PlayerSuit? _playerSuit;
    
    public override void _Ready() {
        _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        if (_player.HasMeta("PlayerSuit")) {
            _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
        }
    }
    public override void _PhysicsProcess(double delta) {
        if (_playerSuit == null) {
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            if (_player.HasMeta("PlayerSuit")) {
                _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
            }
            if (_playerSuit == null) {
                GD.PushError("PassageInUp: _playerSuit is not assigned!");
            }
        } else {
            // 根据玩家状态决定碰撞箱大小和位置
            if (_playerSuit.Suit == PlayerSuit.SuitEnum.Small) {
                _collisionShape2D.Shape = _shapeSmall;
                _collisionShape2D.Position = Position - new Vector2(0f, 12f);
                _collisionShape2D.ResetPhysicsInterpolation();
            } else {
                _collisionShape2D.Shape = _shapeNormal;
                _collisionShape2D.Position = Position - new Vector2(0f, -8f);
                _collisionShape2D.ResetPhysicsInterpolation();
            }
        }
    }
}
