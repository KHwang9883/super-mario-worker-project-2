using Godot;
using System;

public partial class PodobooMovement : Node {
    [Export] private PackedScene _lavaSplashScene = null!;
    [Export] private float _jumpSpeedY = -10f;
    [Export] private float _gravity = 0.25f;
    [Export] private int _resetPhaseTime = 200;
    private Vector2 _originPosition;
    private Node2D? _parent;
    private enum PodobooState {
        Ready,
        Jumping,
        Cooldown,
    }
    private PodobooState _currentState = PodobooState.Ready;
    private int _cooldownCounter;
    private float _speedY;

    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _originPosition = _parent.Position;
    }

    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;
        // 运动
        switch (_currentState) {
            case PodobooState.Ready:
                _currentState = PodobooState.Jumping;
                _speedY = _jumpSpeedY;
                break;
            
            case PodobooState.Jumping:
                _speedY += _gravity;
                _parent.Position = new Vector2(
                    _parent.Position.X,
                    _parent.Position.Y + _speedY
                );

                _parent.Scale = new Vector2(
                    _parent.Scale.X,
                    _speedY < 0 ? 1f : -1f
                );
                
                if (_parent.Position.Y > _originPosition.Y && _speedY > 0) {
                    if (_lavaSplashScene != null) {
                        var lavaSplash = _lavaSplashScene.Instantiate<Node2D>();
                        lavaSplash.Position = new Vector2(
                            _parent.Position.X, _originPosition.Y + 16f
                        );
                        _parent.AddSibling(lavaSplash);
                    }
                    _currentState = PodobooState.Cooldown;
                    _cooldownCounter = 0;
                    _speedY = 0f;
                    _parent.Position = new Vector2(-2000f, -2000f);
                    _parent.ResetPhysicsInterpolation();
                }
                break;
            
            case PodobooState.Cooldown:
                _cooldownCounter++;
                if (_cooldownCounter >= _resetPhaseTime) {
                    _currentState = PodobooState.Ready;
                    _parent.Position = new Vector2(_originPosition.X, _originPosition.Y);
                    _parent.ResetPhysicsInterpolation();
                }
                break;
        }
    }
}
