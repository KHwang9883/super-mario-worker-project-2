using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Projectile.Player;

public partial class TroopaEnemyInteraction : GoombaEnemyInteraction {
    [Export] private NodePath _pathToDieComponent = "../Die";
    // 飞龟需要特别设置生成对象为壳，不能自动获取
    [Export] private PackedScene? _shellStaticScene;
    private EnemyDie _enemyDie = null!;
    private Node2D _parent = null!;
    private CharacterBody2D? _parentShell;
    /*
    [Export] private int _delay = 10;
    private uint _originCollisionLayer;
    */
    private bool _dead;

    public override void _Ready() {
        base._Ready();
        _parent = GetParent<Node2D>();
        _enemyDie = GetNode<EnemyDie>(_pathToDieComponent);
        if (_shellStaticScene == null) {
            _shellStaticScene = GD.Load<PackedScene>(_enemyDie.EnemyDeadPackedSceneUid);
        }
        /*
        if (_parent is CharacterBody2D characterBody2D) {
            _parentShell = characterBody2D;
            _originCollisionLayer = _parentShell.CollisionLayer;
            _parentShell.CollisionLayer = 0;
        }
        */
    }
    /*
    public override void _PhysicsProcess(double delta) {
        if (_delay <= 0) return;
        _delay--;
        if (_delay > 0) return;
        _parentShell?.SetCollisionLayer(_originCollisionLayer);
    }
    */
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
                var shellStatic = _shellStaticScene!.Instantiate<Node2D>();
                shellStatic.Position = _parent.Position;
                _parent.AddSibling(shellStatic);
                shellStatic.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipV = true;
                shellStatic.SetMeta("AnimationFlipV", true);
                var shellStaticInteraction = shellStatic.GetNode<ShellStaticInteraction>("EnemyInteraction");
                shellStaticInteraction.AttackedByTail();
                _parent.QueueFree();
            } else {
                base.OnDied();
            }
        } else {
            base.OnDied();
        }
    }
}
