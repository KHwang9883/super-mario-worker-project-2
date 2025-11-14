using Godot;
using System;
using SMWP.Level.Score;
using SMWP.Level.Tool;

namespace SMWP.Level.Player;

public partial class PlayerDieAndHurt : Node {
    [Signal]
    public delegate void PlayerInvincibleEndedEventHandler();
    [Signal]
    public delegate void PlayerHurtedEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private PackedScene _playerDeadScene = null!;
    [Export] public float InvincibleDuration = 200;
    public bool IsInvincible;
    public bool IsHurtInvincible;
    private int _hurtInvincibleTimer;
    public bool IsStarmanInvincible;
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
        
        // 时间归零死亡
        if (LevelManager.Time == 0) {
            Die();
        }
        
        // 受伤无敌计时
        if (IsHurtInvincible) {
            _hurtInvincibleTimer++;
            if (_hurtInvincibleTimer >= InvincibleDuration) {
                IsHurtInvincible = false;
                EmitSignal(SignalName.PlayerInvincibleEnded);
            }
        }
        
        // 无敌星状态
        IsStarmanInvincible = _playerMediator.playerSuit.Starman;
        
        // 无敌状态标记
        IsInvincible = (IsHurtInvincible || IsStarmanInvincible);
    }
    public CharacterBody2D GetPlayer() {
        return _player;
    }
    public void Die() {
        if (!_dead) {
            _dead = true;
            _player.Visible = false;
            _player.ProcessMode = ProcessModeEnum.Disabled;
            
            LevelManager.Life--;
            
            var playerDeadInstance = _playerDeadScene.Instantiate<PlayerDead>();
            playerDeadInstance.Position = _player.Position;
            _player.AddSibling(playerDeadInstance);
        }
    }
    public void Hurt() {
        if (IsStarmanInvincible || IsHurtInvincible) return;
        IsHurtInvincible = true;
        _hurtInvincibleTimer = 0;
        switch (_playerMediator.playerSuit.Suit) {
            case PlayerSuit.SuitEnum.Small:
                Die();
                break;
            case PlayerSuit.SuitEnum.Super:
                _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Small;
                EmitSignal(SignalName.PlayerHurted);
                break;
            case PlayerSuit.SuitEnum.Powered:
                _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Super;
                EmitSignal(SignalName.PlayerHurted);
                break;
        }
    }
}
