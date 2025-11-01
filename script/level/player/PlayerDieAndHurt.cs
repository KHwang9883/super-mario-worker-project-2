using Godot;
using System;
using SMWP.Level.Enemy;

namespace SMWP.Level.Player;

public partial class PlayerDieAndHurt : Node {
    [Signal]
    public delegate void PlayerHurtEventHandler();
    [Signal]
    public delegate void PlayerInvincibleEndedEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private PackedScene _playerDeadScene = null!;
    [Export] public float InvincibleDuration = 200;
    public bool IsInvicible;
    private int _invincibleTimer;
    private bool _dead;
    private int _deadTimer;

    public override void _PhysicsProcess(double delta) {
        // 死亡计时结束后重启关卡
        if (_dead) {
            _deadTimer++;
            if (_deadTimer >= 180) {
                GetTree().ReloadCurrentScene();
            }
        }
        
        // 掉崖死亡
        float screenBottom = ScreenUtils.GetScreenRect(this).End.Y;
        
        if (_player.GlobalPosition.Y > screenBottom + 30f) {
            Die();
        }
        
        // 无敌计时
        if (IsInvicible) {
            _invincibleTimer++;
            if (_invincibleTimer >= InvincibleDuration) {
                IsInvicible = false;
                EmitSignal(SignalName.PlayerInvincibleEnded);
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
    public void Hurt() {
        switch (IsInvicible) {
            case true:
                return;
            case false:
                IsInvicible = true;
                _invincibleTimer = 0;
                switch (_playerMediator.playerSuit.Suit) {
                    case PlayerSuit.SuitEnum.Small:
                        Die();
                        break;
                    case PlayerSuit.SuitEnum.Super:
                        _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Small;
                        EmitSignal(SignalName.PlayerHurt);
                        break;
                    case PlayerSuit.SuitEnum.Powered:
                        _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Super;
                        EmitSignal(SignalName.PlayerHurt);
                        break;
                }
                break;
        }
    }
}
