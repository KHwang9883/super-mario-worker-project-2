using Godot;
using System;
using SMWP.Level.Enemy;

namespace SMWP.Level.Player;

public partial class PlayerDie : Node {
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private PackedScene _playerDeadScene = null!;
    //private PlayerDead _playerDeadInstance;
    private bool _dead;
    private int _timer;

    public override void _PhysicsProcess(double delta) {
        // 死亡计时结束后重启关卡
        if (_dead) {
            _timer++;
            if (_timer >= 180) {
                GetTree().ReloadCurrentScene();
            }
        }
        
        // 掉崖死亡
        float screenBottom = ScreenUtils.GetScreenRect(this).End.Y;
        
        if (_player.GlobalPosition.Y > screenBottom + 30f) {
            Die();
        }

        // 对Player在PlayerMovement重叠检测的结果进行引用，而非再调用一次ShapeQuery()
        var results = _playerMediator.playerMovement.GetShapeQueryResults();
        
        foreach (var result in results) {
            var trueNode = result.GetNodeOrNull<EnemyBase>("EnemySet");
            if (trueNode == null) { continue; }
            if (trueNode is EnemyBase node) {
                // TODO: 敌人碰撞处理
                if (
                    (node.Stompability == EnemyBase.StompabilityEnum.Unstompable)
                    ||
                    (
                        node.Stompability == EnemyBase.StompabilityEnum.Stompable
                        &&
                        _player.GlobalPosition > node.GlobalPosition
                    )
                ) {
                    switch (node.HurtType)
                    {
                        case EnemyBase.HurtEnum.Die:
                            Die();
                            break;
                        case EnemyBase.HurtEnum.Hurt:
                            //Hurt;
                            break;
                    }
                }
            }
        }
    }

    public CharacterBody2D GetPlayer() {
        return _player;
    }

    public void Die() {
        if (!_dead) {
            _dead = true;
            _player.Visible = false;
            _player.ProcessMode = ProcessModeEnum.Disabled;
            
            var playerDeadInstance = _playerDeadScene.Instantiate<PlayerDead>();
            playerDeadInstance.Position = _player.Position;
            _player.AddSibling(playerDeadInstance);
            
            // 没有实际用途，只是学习过程中留下的代码
            // 问题：为什么下面的代码不会被触发？
            playerDeadInstance.TreeEntered += () => {
                GetTree().Paused = true;
            };
        }
    }
}
