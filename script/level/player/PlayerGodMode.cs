using Godot;
using System;
using SMWP.Level.Player;

public partial class PlayerGodMode : Node {
    [Signal]
    public delegate void AddLifeEventHandler();
    
    [Export] private PlayerMediator? _playerMediator;
    
    public bool IsGodInvincible;
    public bool IsGodFly;

    public override void _Input(InputEvent @event) {
        if (_playerMediator == null) return;
        if (@event is not InputEventKey { Pressed: true } keyEvent) return;
        
        // Todo: God Mode 开启检测
        //if (!LevelManager.IsGodMode) return; ???

        var pSuit = _playerMediator.playerSuit;
        
        switch (keyEvent.Keycode) {
            case Key.Key1:
                pSuit.Suit = PlayerSuit.SuitEnum.Small;
                IsGodFly = false;
                break;
            case Key.Key2:
                pSuit.Suit = PlayerSuit.SuitEnum.Super;
                IsGodFly = false;
                break;
            case Key.Key3:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Fireball;
                IsGodFly = false;
                break;
            case Key.Key4:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Beetroot;
                IsGodFly = false;
                break;
            case Key.Key5:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Lui;
                IsGodFly = false;
                break;
            case Key.Key6:
                pSuit.Starman = true;
                pSuit.StarmanTimer = 0;
                IsGodFly = false;
                break;
            case Key.Key7:
                var pDie = _playerMediator.playerDieAndHurt;
                switch (pDie.IsHurtInvincible) {
                    case true:
                        pDie.HurtEnd();
                        IsGodInvincible = false;
                        break;
                    case false:
                        IsGodInvincible = true;
                        pDie.IsHurtInvincible = true;
                        pDie.HurtInvincibleTimer = 0;
                        break;
                }
                IsGodFly = false;
                break;
            case Key.Key8:
                IsGodFly = !IsGodFly;
                break;
            case Key.Key9:
                EmitSignal(SignalName.AddLife);
                break;
            case Key.Key0:
                // Todo: 滚屏禁用相关
                break;
        }
    }
    public override void _PhysicsProcess(double delta) {
        if (IsGodInvincible && _playerMediator != null)
            _playerMediator.playerDieAndHurt.HurtInvincibleTimer = 0;
    }
}
