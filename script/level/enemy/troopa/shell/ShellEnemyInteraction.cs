using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Physics;
using SMWP.Level.Projectile.Player;
using SMWP.Level.Sound;

public partial class ShellEnemyInteraction : GoombaEnemyInteraction {
    [Signal]
    public delegate void ShellStompedEventHandler(EnemyDie.EnemyDieEnum enemyDieEnum, bool shellSet = false, bool shellDir = false);
    public EnemyDie.EnemyDieEnum EnemyDieType = EnemyDie.EnemyDieEnum.CreateInstance;
    
    [Export] private float _kickSpeedY = -11f;
    [Export] private float _kickSpeedX = 1.1f;
    
    private NodePath _pathToSprite = "AnimatedSprite2D";
    private AnimatedSprite2D _ani = null!;
    private Node2D _parent = null!;
    private BasicMovement _basicMovement = null!;
    private Node2D _player = null!;

    private bool _shellDir;

    private bool _dead;
    
    public override void _Ready() {
        base._Ready();
        _parent = GetParent<Node2D>();
        _basicMovement = GetNode<BasicMovement>("../BasicMovement");
        _ani = _parent.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
    }
    public override float OnStomped(Node2D stomper) {
        if (_dead) {
            return StompSpeedY;
        }
        _dead = true;
        EmitSignal(EnemyInteraction.SignalName.Stomped);
        if (ImmuneToStomp) {
            EmitSignal(EnemyInteraction.SignalName.PlaySoundBumped);
            return StompSpeedY;
        }
        EmitSignal(EnemyInteraction.SignalName.PlaySoundStomped);
        OnNormalAddScore();
        // 被踩以后生成龟壳被踩的类型的尸体，外加方向参数
        var basicMovementComponent = GetParent().GetNodeOrNull<BasicMovement>("BasicMovement");
        if (basicMovementComponent != null) {
            _shellDir = Math.Sign(basicMovementComponent.SpeedX) > 0;
        }
        EmitSignal(SignalName.ShellStomped, Variant.From(EnemyDieType), true, _shellDir);
        //OnDied();
        return StompSpeedY;
    }
    
    public override void OnDied() {
        if (_dead) {
            return;
        }
        _dead = true;
        if (_parent.HasMeta("InteractingObject")) {
            var interactingObj = (Node)_parent.GetMeta("InteractingObject");
            if (interactingObj is Tail) {
                AttackedByTail();
            } else {
                base.OnDied();
            }
        } else {
            base.OnDied();
        }
    }

    public void AttackedByTail() {
        _ani.FlipV = true;
        _parent.SetMeta("AnimationFlipV", _ani.FlipV);
        _basicMovement.SpeedY = _kickSpeedY;
        _basicMovement.SpeedX = 
            (_parent.Position.X < _player.Position.X) ? -Mathf.Abs(_kickSpeedX) : Mathf.Abs(_kickSpeedX);
    }
}
