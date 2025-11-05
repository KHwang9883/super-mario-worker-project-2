using Godot;
using System;

namespace SMWP.Level.Component;

public partial class AutoSpriteFlipH : Node 
{
    // 支持两种类型的精灵
    [Export] private Sprite2D? _sprite2D;
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    
    private int _detectedDirectionX;
    private float _lastPositionX;
    private Node2D? _targetSprite;

    public override void _Ready()
    {
        // 确定使用哪个精灵
        if (_sprite2D != null) {
            _targetSprite = _sprite2D;
        } else if (_animatedSprite2D != null) {
            _targetSprite = _animatedSprite2D;
        }
        
        if (_targetSprite != null) {
            _lastPositionX = _targetSprite.Position.X;
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (_targetSprite == null) return;

        // 检测X方向的变化
        float currentX = _targetSprite.Position.X;
        
        if (_lastPositionX < currentX) 
        {
            _detectedDirectionX = 1; 
        } 
        else if (_lastPositionX > currentX) 
        {
            _detectedDirectionX = -1; 
        }
        
        _lastPositionX = currentX;
        
        // 根据精灵类型设置FlipH
        if (_sprite2D != null) {
            _sprite2D.FlipH = (_detectedDirectionX == -1);
        } else if (_animatedSprite2D != null) {
            _animatedSprite2D.FlipH = (_detectedDirectionX == -1);
        }
    }
}