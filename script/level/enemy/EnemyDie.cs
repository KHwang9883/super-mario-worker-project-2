using Godot;
using System;

namespace SMWP.Level.Enemy;

public partial class EnemyDie : Node {
    [Signal]
    public delegate void DiedEventHandler();
    
    enum EnemyDieEnum {
        Disappear,
        Explode,
        SpinOff,
        CreateInstance,
    }
    [Export] private EnemyDieEnum _enemyDieType = EnemyDieEnum.Disappear;
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private PackedScene _fireballExplosionScene = GD.Load<PackedScene>("uid://5mmyew6mh71p");
    [Export] private PackedScene _enemyDeadNormalPackedScene = GD.Load<PackedScene>("uid://ctlj6wtkwhahy");
    [Export] private PackedScene? _enemyDeadPackedScene;
    
    private Node2D _parent = null!;
    // 尸体为敌人动画第一帧精灵内容
    private Texture2D? _texture2D;

    public override void _Ready() {
        _parent = GetParent<Node2D>();
        // 储存第一帧精灵内容
        _texture2D = _animatedSprite2D.SpriteFrames.GetFrameTexture(_animatedSprite2D.Animation, _animatedSprite2D.Frame);
    }
    public virtual void OnDied() {
        EmitSignal(SignalName.Died);
        //Callable.From(() => {
        switch (_enemyDieType) {
            case EnemyDieEnum.Explode:
                var fireballExplosionInstance = _fireballExplosionScene.Instantiate<Node2D>();
                fireballExplosionInstance.Position = _parent.Position;
                _parent.AddSibling(fireballExplosionInstance);
                break;
            case EnemyDieEnum.SpinOff:
                var enemyDeadNormalInstance = _enemyDeadNormalPackedScene.Instantiate<Node2D>();
                enemyDeadNormalInstance.Position = _parent.Position;
                enemyDeadNormalInstance.GetNode<Sprite2D>("Sprite2D").Texture = _texture2D;
                _parent.AddSibling(enemyDeadNormalInstance);
                break;
            case EnemyDieEnum.CreateInstance: {
                if (_enemyDeadPackedScene != null) {
                    var enemyDeadInstance = _enemyDeadPackedScene.Instantiate<Node2D>();
                    enemyDeadInstance.Position = _parent.Position;
                    _parent.AddSibling(enemyDeadInstance);
                }
                break;
            }
        }
        _parent.QueueFree();
        //});
    }
}
