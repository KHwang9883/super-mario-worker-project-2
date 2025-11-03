using Godot;
using System;

public partial class MushroomAnimation : Node {
    [Signal]
    public delegate void SideTurnedEventHandler();
    
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private MushroomMovement _mushroomMovement = null!;
    [Export] private CharacterBody2D _mushroom = null!;
    
    private int _blinkingTimer;
    private int _randomPercent;
    private RandomNumberGenerator _random = new RandomNumberGenerator();
    private bool _playingAnimation;

    private enum ScaleXStatus {
        Idle = 0,
        Prepare = 1,
        ScalingUp = 2,
        ScalingDown = 3
    }
    private ScaleXStatus _animationFrameScaleXStatus = ScaleXStatus.Idle;
    public int AnimationFrameScaleX;
    
    private bool _wasOnFloor = true;
    private enum ScaleYStatus {
        Idle = 0,
        Prepare = 1,
        ScalingUp = 2,
        ScalingDown = 3
    }
    private ScaleYStatus _animationFrameScaleYStatus;
    public int AnimationFrameScaleY;
    
    public override void _PhysicsProcess(double delta) {
        // 眨眼动画
        if (!_playingAnimation) {
            _blinkingTimer++;
            if (_blinkingTimer >= 20) {
                _randomPercent = _random.RandiRange(1, 100);
                if (_randomPercent <= 60) {
                    _animatedSprite2D.Play("default");
                    _playingAnimation = true;
                }
                _blinkingTimer = 0;
            }
        }

        // 撞墙动画
        if (_mushroomMovement.Turning) {
            if (_animationFrameScaleXStatus == ScaleXStatus.Idle) {
                _animationFrameScaleXStatus = ScaleXStatus.Prepare;
            }
        }
        
        switch (_animationFrameScaleXStatus) {
            case ScaleXStatus.Prepare:
                _animationFrameScaleXStatus = ScaleXStatus.ScalingUp;
                AnimationFrameScaleX = 0;
                break;
            case ScaleXStatus.ScalingUp when AnimationFrameScaleX < 10:
                AnimationFrameScaleX += 1;
                break;
            case ScaleXStatus.ScalingUp:
                _animationFrameScaleXStatus = ScaleXStatus.ScalingDown;
                break;
            case ScaleXStatus.ScalingDown when AnimationFrameScaleX > 0:
                AnimationFrameScaleX -= 1;
                break;
            case ScaleXStatus.ScalingDown:
                _animationFrameScaleXStatus = ScaleXStatus.Idle;
                EmitSignal(SignalName.SideTurned);
                break;
        }
        
        // 落地动画
        bool isOnFloor = _mushroom.IsOnFloor();
        if (isOnFloor && !_wasOnFloor) {
            _animationFrameScaleYStatus = ScaleYStatus.Prepare;
        }
        _wasOnFloor = isOnFloor;
        
        switch (_animationFrameScaleYStatus) {
            case ScaleYStatus.Prepare:
                _animationFrameScaleYStatus = ScaleYStatus.ScalingUp;
                AnimationFrameScaleY = 0;
                break;
            case ScaleYStatus.ScalingUp when AnimationFrameScaleY < 10:
                AnimationFrameScaleY += 1;
                break;
            case ScaleYStatus.ScalingUp:
                _animationFrameScaleYStatus = ScaleYStatus.ScalingDown;
                break;
            case ScaleYStatus.ScalingDown when AnimationFrameScaleY > 0:
                AnimationFrameScaleY -= 1;
                break;
            case ScaleYStatus.ScalingDown:
                _animationFrameScaleYStatus = ScaleYStatus.Idle;
                break;
        }
        
        // 应用 Scale 和 Position 变化
        _animatedSprite2D.Scale = new Vector2(1f - AnimationFrameScaleX / 20f, 1f - AnimationFrameScaleY / 20f);
        _animatedSprite2D.GlobalPosition = new Vector2(
            _mushroom.GlobalPosition.X + AnimationFrameScaleX * 1.0f * Mathf.Sign(_mushroomMovement.SpeedX),
            _animatedSprite2D.GlobalPosition.Y);
    }
    
    // 眨眼动画结束回调
    public void OnAnimationFinished() {
        _playingAnimation = false;
    }
}