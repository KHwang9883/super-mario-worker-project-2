using Godot;
using System;

public partial class EnemyDie : Node {
    enum EnemyDieEnum {
        Disappear,
        CreateInstance
    }
    [Export] private EnemyDieEnum _enemyDieType = EnemyDieEnum.Disappear;
    [Export] private PackedScene _enemyDeadPackedScene = null!;
    
    public void OnCreateDeadInstance(Node2D ancestor, Vector2 position) {
        if (_enemyDieType == EnemyDieEnum.CreateInstance) {
            var enemyDeadInstance = _enemyDeadPackedScene.Instantiate<Node2D>();
            enemyDeadInstance.Position = position;
            ancestor.AddSibling(enemyDeadInstance);
        }
    }
}
