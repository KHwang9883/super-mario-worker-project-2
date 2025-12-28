using Godot;
using System;
using SMWP.Level.Score;
using SMWP.Level.Sound;
using SMWP.Util;

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
    [Export] public float HurtInvincibleDuration = 200;
    [Export] public int ExtraInvincibleTime = 10;
    [Export] private ContinuousAudioStream2D? _gameOverSound;
    [Export] private PackedScene _fireballExplosion = GD.Load<PackedScene>("uid://5mmyew6mh71p");
    
    public bool IsInvincible;
    
    public bool IsHurtInvincible;
    public int HurtInvincibleTimer;
    
    public bool IsStarmanInvincible;

    public bool IsExtraInvincible;
    private int _extraInvincibleTimer;
    
    private LevelConfig? _levelConfig;
    
    private bool _dead;
    private int _deadTimer;

    private LevelCamera? _levelCamera;
    private float _screenBottom = 9999999f;
    private int _playerInScreenBottomTimer;
    private int _initialFrameDelay;
    
    private ScreenFreeze _screenFreeze = null!;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _levelCamera ??= (LevelCamera)GetTree().GetFirstNodeInGroup("camera");
        
        _screenFreeze = GetTree().Root.GetNode<ScreenFreeze>("ScreenFreeze");
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
            
            // 冻结画面用，防止重新加载关卡准备完毕前画面渲染导致露馅
            if (_deadTimer >= deadTime - 2 && GameManager.Life > 0) {   // 别问，问就是给延迟留足时间
                _screenFreeze.SetFreeze();
            }
            
            if (_deadTimer >= deadTime) {
                // 设置状态全局记录
                GameManager.PlayerSuitRestore = _playerMediator.playerSuit.Suit;
                GameManager.PlayerPowerupRestore = _playerMediator.playerSuit.Powerup;
                
                // Restart Level
                if (GameManager.Life > 0) {
                    GetTree().ReloadCurrentScene();
                }
                
                // Game Over
                else {
                    if (!GameManager.IsGameOver) {
                        GameManager.IsGameOver = true;
                        if (!_levelConfig.FastRetry) {
                            EmitSignal(SignalName.PlaySoundGameOver);
                        }
                    }
                    
                    var gameOverTime = !_levelConfig.FastRetry ? 500 : 250;
                    if (_deadTimer >= gameOverTime && Input.IsAnythingPressed()) {
                        
                        // Todo: 跳转到编辑界面或者标题界面
                        
                        GameManager.GameOverClear();
                        var gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
                        gameManager.JumpToLevel();
                    }
                }
            }
        }
        
        // 掉崖死亡
        _screenBottom = ScreenUtils.GetScreenRect(this).End.Y;
        if (_initialFrameDelay < 10) {
            _initialFrameDelay++;
        } else {
            if (_player.GlobalPosition.Y > _screenBottom + 30f) {
                //GD.Print("Dead by cliff.");
                Die();
            }
        }
        
        // 时间归零死亡
        if (GameManager.Time == 0) {
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
            if (HurtInvincibleTimer >= HurtInvincibleDuration) {
                HurtEnd();
            }
        }
        
        // 无敌星状态
        IsStarmanInvincible = _playerMediator.playerSuit.Starman;
        
        // 额外无敌时间计时
        if (IsExtraInvincible) {
            if (_extraInvincibleTimer > 0) {
                _extraInvincibleTimer--;
            } else {
                IsExtraInvincible = false;
            }
        }
        
        // 无敌状态标记
        IsInvincible = (IsHurtInvincible || IsStarmanInvincible || IsExtraInvincible);
        
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
        
        // 扣命
        GameManager.Life--;
            
        var playerDeadInstance = _playerDeadScene.Instantiate<PlayerDead>();
        playerDeadInstance.Position = _player.Position;
        _player.AddSibling(playerDeadInstance);
    }
    public void Hurt() {
        if (IsStarmanInvincible || IsHurtInvincible || IsExtraInvincible) return;
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

    public void SetStompInvincibleTime() {
        _extraInvincibleTimer = ExtraInvincibleTime;
        IsExtraInvincible = true;
    }
}
