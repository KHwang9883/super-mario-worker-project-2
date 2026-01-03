using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Physics;
using SMWP.Level.Projectile.Player;

public partial class ShellStaticInteraction : GoombaEnemyInteraction {
    [Export] private int _delay = 10;
    
    [Export] private float _kickSpeedY = -11f;
    [Export] private float _kickSpeedX = 1.1f;
    
    private NodePath _pathToSprite = "AnimatedSprite2D";
    private AnimatedSprite2D _ani = null!;
    private CharacterBody2D _parent = null!;
    private BasicMovement _basicMovement = null!;
    private Node2D _player = null!;
    private uint _originCollisionLayer;

    private bool _dead;
    
    public override void _Ready() {
        base._Ready();
        _parent = GetParent<CharacterBody2D>();
        _basicMovement = GetNode<BasicMovement>("../BasicMovement");
        _ani = _parent.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        _originCollisionLayer = _parent.CollisionLayer;
        _parent.CollisionLayer = 0;
    }
    public override void _PhysicsProcess(double delta) {
        if (_delay <= 0) return;
        _delay--;
        if (_delay > 0) return;
        /*
        _enemyInteractionComponent.HurtType = IHurtableAndKillable.HurtEnum.Hurt;
        */
        _parent.CollisionLayer = _originCollisionLayer;
    }
    
    public override void PlayerHurtCheck(bool check) {
        if (_ani.FlipV) {
            _parent.SetMeta("AnimationFlipV", true);
        }
        EmitSignal(EnemyInteraction.SignalName.PlaySoundStomped);
        EmitSignal(GoombaEnemyInteraction.SignalName.GoombaStomped, Variant.From(EnemyDieType));
    }

    public override float OnStomped(Node2D stomper) {
        if (_dead) {
            return StompSpeedY;
        }
        _dead = true;
        return base.OnStomped(stomper);
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
