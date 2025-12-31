using Godot;
using System;
using SMWP.Level.Player;

namespace SMWP.Level.Player;

public partial class PlayerSuit : Node {
    [Signal]
    public delegate void PlayerStarmanStartedEventHandler();
    [Signal]
    public delegate void PlayerStarmanFinishedEventHandler();
    [Signal]
    public delegate void PlayerStarmanFinishedBgmResetEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    
    [Export] private ComboComponent _starmanCombo = null!;

    [Export] private SmwpPointLight2D _smwpPointLight2D = null!;
    
    public enum SuitEnum {
        Small,
        Super,
        Powered,
    }
    [Export] public SuitEnum Suit = SuitEnum.Small;
    public enum PowerupEnum {
        Fireball,
        Beetroot,
        Lui,
        Raccoon,
    }
    [Export] public PowerupEnum Powerup = PowerupEnum.Fireball;
    [Export] public bool Starman;
    [Export] public int StarmanTime { get; set; } = 500;
    public int StarmanTimer { get; set; }

    // 玩家光源与无敌星状态光源一共有两个独立的标记
    private LevelConfig? _levelConfig;
    [Export] private bool _playerLight;
    [Export] private bool _starmanLight;

    public override void _Ready() {
        Suit = GameManager.PlayerSuitRestore;
        Powerup = GameManager.PlayerPowerupRestore;

        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        Callable.From(() => {
            if (_levelConfig.Smwp1LightObjectString[1 - 1] == '1') {
                _playerLight = true;
            }
            if (_levelConfig.Smwp1LightObjectString[61 - 1] == '1') {
                _starmanLight = true;
            }
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (GameManager.IsLevelPass) {
            GameManager.PlayerSuitRestore = Suit;
            GameManager.PlayerPowerupRestore = Powerup;
        }
        
        // 玩家光源与无敌星状态光源
        /*if (!_playerLight && !Starman) {
            _smwpPointLight2D.LightRadius = 0f;
        }*/
        
        // 无敌星状态光源半径变化
        _smwpPointLight2D.LightRadius =
            Mathf.MoveToward(_smwpPointLight2D.LightRadius,
                (_starmanLight && Starman) ? 35f : (_playerLight ? 1f : 0f), 0.6f);

        if (!Starman) return;
        
        // 传送状态下无敌星计时暂停
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        
        StarmanTimer++;
        if (StarmanTimer < StarmanTime) return;
        StarmanOver();
    }
    
    public void OnPlayerDied() {
        // 变为小个子
        _playerMediator.playerSuit.Suit = SuitEnum.Small;
        // 这里全局的记录玩家状态的变量延迟更新，以复刻先天发育效果
    }
    
    public void SetStarmanState() {
        StarmanTimer = 0;
        if (Starman) return;
        Starman = true;
        EmitSignal(SignalName.PlayerStarmanStarted);
    }
    public void StarmanOver(bool bgmReset = true) {
        Starman = false;
        StarmanTimer = 0;
        _starmanCombo.ResetCombo();
        EmitSignal(SignalName.PlayerStarmanFinished);
        if (bgmReset) EmitSignal(SignalName.PlayerStarmanFinishedBgmReset);
    }
}
