using Godot;
using System;

namespace SMWP.Level.Component;

[GlobalClass]
public partial class AutoSpriteFlipH : Node 
{
    // 支持两种类型的精灵
    [Export] private Sprite2D? _sprite2D;
    [Export] private AnimatedSprite2D? _animatedSprite2D;
    [Export] private bool _alwaysFaceToPlayer;
    
    private int _detectedDirectionX;
    private float _lastPositionX;
    private Node2D? _targetSprite;
    private bool _flipH;

    public override void _Ready()
    {
        // 确定使用哪个精灵
        if (_sprite2D != null) {
            _targetSprite = _sprite2D;
        } else if (_animatedSprite2D != null) {
            _targetSprite = _animatedSprite2D;
        }
        
        if (_targetSprite != null) {
            _lastPositionX = _targetSprite.GlobalPosition.X;
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (_targetSprite == null) return;

        float currentX = _targetSprite.GlobalPosition.X;
        
        // 永远面朝玩家
        if (_alwaysFaceToPlayer) {
            Node2D player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            if (currentX < player.GlobalPosition.X) {
                _flipH = false;
            } else if (currentX > player.GlobalPosition.X) {
                _flipH = true;
            }
            if (_sprite2D != null) {
                _sprite2D.FlipH = _flipH;
            } else if (_animatedSprite2D != null) {
                _animatedSprite2D.FlipH = _flipH;
            }
            return;
        }
        
        // 检测X方向的变化
        if (_lastPositionX < currentX) {
            _detectedDirectionX = 1; 
        } else if (_lastPositionX > currentX) {
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