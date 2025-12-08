using Godot;
using System;
using SMWP.Level.Score;
using SMWP.Level.Sound;
using SMWP.Level.Tool;

namespace SMWP.Level.Player;

public partial class PlayerDieAndHurt : Node {
    [Signal]
    public delegate void PlayerInvincibleEndedEventHandler();
    [Signal]
    public delegate void PlayerDiedEventHandler();
    [Signal]
    public delegate void PlayerHurtedEventHandler();
    [Signal]
    public delegate void PlaySoundGameOverEventHandler();
    [Signal]
    public delegate void PlaySoundBreakEventHandler();
    [Signal]
    public delegate void PlayerDiedSucceededEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private PackedScene _playerDeadScene = null!;
    [Export] public float InvincibleDuration = 200;
    [Export] private ContinuousAudioStream2D? _gameOverSound;
    [Export] private PackedScene _fireballExplosion = GD.Load<PackedScene>("uid://5mmyew6mh71p");
    
    public bool IsInvincible;
    public bool IsHurtInvincible;
    public int HurtInvincibleTimer;
    public bool IsStarmanInvincible;
    private LevelConfig? _levelConfig;
    private bool _dead;
    private int _deadTimer;

    private LevelCamera? _levelCamera;
    private float _screenBottom = 9999999f;
    private int _playerInScreenBottomTimer;
    private int _initialFrameDelay;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _levelCamera ??= (LevelCamera)GetTree().GetFirstNodeInGroup("camera");
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        // 水管传送状态下不处理
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        
        // 死亡计时结束后重启关卡
        if (_dead) {
            _deadTimer++;
            var deadTime = !_levelConfig.FastRetry ? 180 : 90;
            if (_deadTimer >= deadTime) {
                // Restart Level
                if (LevelManager.Life > 0) {
                    GetTree().ReloadCurrentScene();
                }
                
                // Game Over
                else {
                    if (!LevelManager.IsGameOver) {
                        LevelManager.IsGameOver = true;
                        if (!_levelConfig.FastRetry) {
                            EmitSignal(SignalName.PlaySoundGameOver);
                        }
                    }
                    
                    var gameOverTime = !_levelConfig.FastRetry ? 500 : 250;
                    if (_deadTimer >= gameOverTime && Input.IsAnythingPressed()) {
                        
                        // Todo: 跳转到编辑界面或者标题界面
                        
                        LevelManager.GameOverClear();
                        GetTree().ChangeSceneToFile("uid://2h2s1iqemydd");
                    }
                }
            }
        }
        
        // 掉崖死亡
        if (_initialFrameDelay < 10) {
            _initialFrameDelay++;
        } else {
            if (_player.GlobalPosition.Y > _screenBottom + 30f) {
                Die();
            }
        }
        _screenBottom = ScreenUtils.GetScreenRect(this).End.Y;
        //GD.Print(_screenBottom);
        
        // 时间归零死亡
        if (LevelManager.Time == 0) {
            Die();
        }
        
        // 按自爆键死亡
        if (!_dead
            && Input.IsActionPressed("move_restart_level")
            && !_playerMediator.playerGodMode.IsGodFly) {
            Die();
            var fireballExplosion = _fireballExplosion.Instantiate<Node2D>();
            fireballExplosion.Position = _player.Position + Vector2.Down * 16f;
            _player.AddSibling(fireballExplosion);
            EmitSignal(SignalName.PlaySoundBreak);
        }
        
        // 受伤无敌计时
        if (IsHurtInvincible) {
            HurtInvincibleTimer++;
            if (HurtInvincibleTimer >= InvincibleDuration) {
                HurtEnd();
            }
        }
        
        // 无敌星状态
        IsStarmanInvincible = _playerMediator.playerSuit.Starman;
        
        // 无敌状态标记
        IsInvincible = (IsHurtInvincible || IsStarmanInvincible);
        
        // 首帧判定延迟
        //if (!_initialFrameDelay) _initialFrameDelay = true;
    }
    public CharacterBody2D GetPlayer() {
        return _player;
    }
    public void Die() {
        if (_playerMediator.playerGodMode.IsGodFly) return;
        if (_dead) return;
        _dead = true;
        _player.Visible = false;
        _player.ProcessMode = ProcessModeEnum.Disabled;
        EmitSignal(SignalName.PlayerDiedSucceeded);

        // 变为小个子
        _playerMediator.playerSuit.Suit = PlayerSuit.SuitEnum.Small;
        
        // 扣命
        LevelManager.Life--;
            
        var playerDeadInstance = _playerDeadScene.Instantiate<PlayerDead>();
        playerDeadInstance.Position = _player.Position;
        _player.AddSibling(playerDeadInstance);
    }
    public void Hurt() {
        if (IsStarmanInvincible || IsHurtInvincible) return;
        SetHurtInvincible();
        switch (_playerMediator.playerSuit.Suit) {
            case PlayerSuit.SuitEnum.Small:
                Die();
                EmitSignal(SignalName.PlayerDied);
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
    public void SetHurtInvincible() {
        IsHurtInvincible = true;
        HurtInvincibleTimer = 0;
    }
    public void HurtEnd() {
        IsHurtInvincible = false;
        HurtInvincibleTimer = 0;
        EmitSignal(SignalName.PlayerInvincibleEnded);
    }
}
