using Godot;
using System;

namespace SMWP.Level.Enemy;

public partial class EnemyDie : Node {
    [Signal]
    public delegate void DiedEventHandler();
    
    enum EnemyDieEnum {
        Disappear,
        CreateInstance,
    }
    [Export] private EnemyDieEnum _enemyDieType = EnemyDieEnum.Disappear;
    [Export] private PackedScene _enemyDeadPackedScene = null!;
    private Node2D _parent = null!;

    public override void _Ready() {
        _parent = GetParent<Node2D>();
    }
    public virtual void OnDied() {
        EmitSignal(SignalName.Died);
        //Callable.From(() => {
        if (_enemyDieType == EnemyDieEnum.CreateInstance) {
            var enemyDeadInstance = _enemyDeadPackedScene.Instantiate<Node2D>();
            enemyDeadInstance.Position = _parent.Position;
            _parent.AddSibling(enemyDeadInstance);
        }
        _parent.QueueFree();
        //});
    }
}
