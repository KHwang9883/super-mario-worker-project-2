using Godot;
using System;
using SMWP.Level.Sound;
using SMWP.Level.Tool;

public partial class LakituMovement : Node {
    [Export] private float _trackDistance = 150f;
    [Export] private float _reloadTime = 300f;
    [Export] private float _readyTime = 90f;
    [Export] private PackedScene _spinyBallScene = GD.Load<PackedScene>("uid://bbxcifwplt1gj");
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private ContinuousAudioStream2D _lakituThrowSounds = null!;
    
    public enum LakituState { Reload, Ready, Threw }
    private LakituState _currentState = LakituState.Reload;
    private float _timer;
    private float _timeAddSpeedImproved;
    
    private Node2D? _parent;
    private Node2D? _player;
    private float _speedX;
    private RandomNumberGenerator _rng = new ();
    
    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _animatedSprite2D.AnimationFinished += OnAnimationReadyFinished;
        _timeAddSpeedImproved = 1f + _rng.RandfRange(0f, 1f);
    }
    public override void _PhysicsProcess(double delta) {
        // Movement
        if (_parent == null || _player == null) return;

        var posX = _parent.Position.X;
        var playerX = _player.Position.X;
        var distance = posX - playerX;
        var absDistance = Math.Abs(distance);

        _speedX = Mathf.Clamp(_speedX, -12f, 12f);

        if (absDistance > _trackDistance) {
            if ((distance > 0 && _speedX > 0) || (distance < 0 && _speedX < 0)) {
                _speedX -= 0.2f * Math.Sign(distance);
            } else {
                _speedX -= 0.1f * Math.Sign(distance);
            }
        } else {
            if (distance > 0 && _speedX > -4f) {
                _speedX -= 0.1f;
            }
            else if (distance < 0 && _speedX < 4f) {
                _speedX += 0.1f;
            }
        }

        _parent.Position = _parent.Position with { X = _parent.Position.X + _speedX };
        
        // Attack States
        switch (_currentState) {
            case LakituState.Reload:
                _timer += _timeAddSpeedImproved;
                if (_rng.RandiRange(0, 100) == 1 && _animatedSprite2D.Animation != "blink")
                    _animatedSprite2D.Play("blink");
                if (_timer < _reloadTime) break;
                _timer = 0f;
                _currentState = LakituState.Ready;
                _animatedSprite2D.Play("throw");
                break;
            case LakituState.Ready:
                _timer++;
                if (_timer < _readyTime) break;
                _currentState = LakituState.Threw;
                _animatedSprite2D.PlayBackwards("throw");
                Throw();
                break;
        }
    }
    public void Throw() {
        if (_parent == null) return;
        
        var spinyBall = _spinyBallScene.Instantiate<Node2D>();
        spinyBall.Position = _parent.Position + Vector2.Up * 18f;
        _parent.AddSibling(spinyBall);

        Rect2 screen = ScreenUtils.GetScreenRect(this);
        
        if (_parent.Position.X < screen.Position.X - 16f 
            || _parent.Position.X > screen.End.X + 16f
            || _parent.Position.Y < screen.Position.Y - 24f
            || _parent.Position.Y > screen.End.Y + 24f) return;
        _lakituThrowSounds.Play();
    }
    public void OnAnimationReadyFinished() {
        if (_animatedSprite2D.Animation == "blink") _animatedSprite2D.Animation = "default";
        if (_animatedSprite2D.Animation != "throw") return;
        switch (_currentState) {
            case LakituState.Ready:
                //_animatedSprite2D.Stop();
                break;
            case LakituState.Threw:
                _currentState = LakituState.Reload;
                _timeAddSpeedImproved = 1f + _rng.RandfRange(0f, 1f); 
                _animatedSprite2D.Animation = "default";
                break;
        }
    }
}
