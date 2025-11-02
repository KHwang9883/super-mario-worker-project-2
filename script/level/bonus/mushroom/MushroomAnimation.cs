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

    private bool _wasOnFloor = true;
    private enum ScaleStatus 
    {
        Idle = 0,
        Prepare = 1,
        ScalingUp = 2,
        ScalingDown = 3
    }
    private ScaleStatus _animationFrameScaleYStatus;
    private int _animationFrameScaleY;
    
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
            // ...
            EmitSignal(SignalName.SideTurned);
        }
        
        // 落地动画 - 检测从空中到地面的瞬间
        bool isOnFloor = _mushroom.IsOnFloor();
        if (isOnFloor && !_wasOnFloor) {
            _animationFrameScaleYStatus = ScaleStatus.Prepare;
        }
        _wasOnFloor = isOnFloor;
        
        // 缩放动画状态机
        switch (_animationFrameScaleYStatus) {
            case ScaleStatus.Prepare:
                _animationFrameScaleYStatus = ScaleStatus.ScalingUp;
                _animationFrameScaleY = 0;
                break;
            case ScaleStatus.ScalingUp when _animationFrameScaleY < 10:
                _animationFrameScaleY += 1;
                break;
            case ScaleStatus.ScalingUp:
                _animationFrameScaleYStatus = ScaleStatus.ScalingDown;
                break;
            case ScaleStatus.ScalingDown when _animationFrameScaleY > 0:
                _animationFrameScaleY -= 1;
                break;
            case ScaleStatus.ScalingDown:
                _animationFrameScaleYStatus = ScaleStatus.Idle;
                break;
        }
        
        // 应用 Scale 变化
        _animatedSprite2D.Scale = new Vector2(1f, 1f - _animationFrameScaleY / 20f);
    }
    
    // 眨眼动画结束回调
    public void OnAnimationFinished() {
        _playingAnimation = false;
    }
}