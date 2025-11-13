using Godot;
using System;
using SMWP.Level.Player;

public partial class BooMovement : Node {
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    private Node2D? _parent;
    private float _originPositionY;
    private Node2D? _player;
    private PlayerMovement _playerMovement = null!;
    //private float _direction;
    private bool _move;
    private float _phase;
    
    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _originPositionY = _parent.Position.Y;
        _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        if (_player.HasMeta("PlayerMovement")) {
            _playerMovement = (PlayerMovement)_player.GetMeta("PlayerMovement");
        }
        _parent.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = _parent.Position.X > _player.Position.X;
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null || _player == null) return;
        
        if (_parent.Position.X > _player.Position.X) {
            _move = (_playerMovement.Direction != 1);
        } else {
            _move = (_playerMovement.Direction != -1);
        }
        
        // Animation
        if (!_move) {
            _animatedSprite2D.Animation = "default";
        } else {
            _animatedSprite2D.Animation = "track";
        }
        
        if (!_move) return;
        
        // x 运动
        if (_parent.Position.X > _player.Position.X) {
            _parent.Position = _parent.Position with { X = _parent.Position.X - 0.8f};
        }
        if (_parent.Position.X < _player.Position.X) {
            _parent.Position = _parent.Position with { X = _parent.Position.X + 0.8f};
        }
        
        // y 运动
        _parent.Position = _parent.Position with { Y = _originPositionY - Mathf.Sin(_phase) * 30f};
        _phase += 0.04f;
        _parent.Position = _parent.Position with { Y = _originPositionY + Mathf.Sin(_phase) * 30f};
        if (_originPositionY < _player.Position.Y) {
            _originPositionY += 0.2f;
        }
        if (_originPositionY > _player.Position.Y) {
            _originPositionY -= 0.2f;
        }
    }
}
