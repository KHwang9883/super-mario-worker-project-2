using Godot;
using System;
using SMWP.Level.Enemy.Troopa.Shell;

namespace SMWP.Level.Enemy;

public partial class EnemyDie : Node {
    [Signal]
    public delegate void DiedEventHandler();

    public enum EnemyDieEnum {
        Disappear,
        Explode,
        SpinOff,
        CreateInstance,
    }
    [Export] private EnemyDieEnum _enemyDieType = EnemyDieEnum.Disappear;
    [Export] private AnimatedSprite2D _animatedSprite2D = null!;
    [Export] private PackedScene _fireballExplosionScene = GD.Load<PackedScene>("uid://5mmyew6mh71p");
    [Export] private PackedScene _enemyDeadNormalPackedScene = GD.Load<PackedScene>("uid://ctlj6wtkwhahy");
    [Export] private AtlasTexture? _enemyDeadNormalTextureOverride;
    
    // 进行特定交互方式生成的物件
    [Export] public StringName? EnemyDeadPackedSceneUid { get; private set; }
    private PackedScene? _enemyDeadPackedScene;
    
    private Node2D _parent = null!;
    // 尸体为敌人动画第一帧精灵内容
    private Texture2D? _texture2D;

    // 防止多次死亡（比如一帧内受到多种攻击）
    public bool Dead;
    
    public override void _Ready() {
        _parent = GetParent<Node2D>();
        // 储存第一帧精灵内容
        _texture2D = _animatedSprite2D.SpriteFrames.GetFrameTexture(_animatedSprite2D.Animation, _animatedSprite2D.Frame);
        
        // 进行特定交互方式生成的物件
        if (EnemyDeadPackedSceneUid == null) return;
        _enemyDeadPackedScene = GD.Load<PackedScene>(EnemyDeadPackedSceneUid);
    }
    public virtual void OnDied() {
        OnDied(_enemyDieType, false, false);
    }
    public void OnDied(EnemyDieEnum enemyDieType) {
        OnDied(enemyDieType, false , false);
    }
    public virtual void OnDied(EnemyDieEnum enemyDieType, bool shellSet = false, bool shellDir = false) {
        if (Dead) return;
        Dead = true;
        EmitSignal(SignalName.Died);
        Callable.From(() => {
        switch (enemyDieType) {
            case EnemyDieEnum.Explode:
                var fireballExplosionInstance = _fireballExplosionScene.Instantiate<Node2D>();
                fireballExplosionInstance.Position = _parent.Position;
                _parent.AddSibling(fireballExplosionInstance);
                if (!_parent.HasMeta("InteractingObject")) break;
                fireballExplosionInstance.SetMeta("InteractingObject", Variant.From(_parent.GetMeta("InteractingObject")));
                break;
            case EnemyDieEnum.SpinOff:
                var enemyDeadNormalInstance = _enemyDeadNormalPackedScene.Instantiate<Node2D>();
                enemyDeadNormalInstance.Position = _parent.Position;
                enemyDeadNormalInstance.GetNode<Sprite2D>("Sprite2D").Texture
                    = (_enemyDeadNormalTextureOverride == null) ? _texture2D : _enemyDeadNormalTextureOverride;
                _parent.AddSibling(enemyDeadNormalInstance);
                if (!_parent.HasMeta("InteractingObject")) break;
                enemyDeadNormalInstance.SetMeta("InteractingObject", Variant.From(_parent.GetMeta("InteractingObject")));
                break;
            case EnemyDieEnum.CreateInstance:
                if (_enemyDeadPackedScene != null) {
                    var enemyDeadInstance = _enemyDeadPackedScene.Instantiate<Node2D>();
                    enemyDeadInstance.Position = _parent.Position;
                    _parent.AddSibling(enemyDeadInstance);
                    
                    if (shellSet) {
                        enemyDeadInstance.SetMeta("ShellSwitchDirection", shellDir);
                        
                    }
                    
                    // 龟壳被浣熊装打贴图倒置的情况
                    if (_parent.HasMeta("AnimationFlipV")) {
                        enemyDeadInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipV = true;
                        enemyDeadInstance
                            .GetNodeOrNull<ShellMovingPlayerInteractionDelay>("InteractionWithPlayerDelay")
                            ?.SetDelay(50);
                        enemyDeadInstance.SetMeta("AnimationFlipV", true);
                    }
                    
                    if (!_parent.HasMeta("InteractingObject")) break;
                    enemyDeadInstance.SetMeta("InteractingObject", Variant.From(_parent.GetMeta("InteractingObject")));
                }
                break;
        }
        _parent.QueueFree();
        }).CallDeferred();
    }
}
