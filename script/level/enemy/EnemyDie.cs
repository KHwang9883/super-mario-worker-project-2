using Godot;
using System;

public partial class EnemyDie : Node {
    enum EnemyDieEnum {
        Disappear,
        CreateInstance
    }
    [Export] private EnemyDieEnum _enemyDieType = EnemyDieEnum.Disappear;
    [Export] private PackedScene _enemyDeadPackedScene = null!;
    
    private Node2D _ancestor = null!;

    public override void _Ready() {
        _ancestor = GetParent<Node2D>();
    }
    public void OnCreateDeadInstance(Vector2 position) {
        if (_enemyDieType == EnemyDieEnum.CreateInstance) {
            var enemyDeadInstance = _enemyDeadPackedScene.Instantiate<Node2D>();
            enemyDeadInstance.Position = position;
            _ancestor.AddSibling(enemyDeadInstance);
        }
    }
}
