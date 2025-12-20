using Godot;
using System;
using Godot.Collections;
using SMWP;
using SMWP.Level;
using SMWP.Level.Player;

public partial class PlayerGodMode : Node {
    [Signal]
    public delegate void AddLifeEventHandler();
    
    [Export] private PlayerMediator? _playerMediator;
    public bool IsGodMode;
    
    public bool IsGodInvincible;
    public bool IsGodFly;
    public bool ForceScrollDisabled;
    
    private Array<Node>? _checkpoints;
    private LevelCamera? _levelCamera;

    public override void _Ready() {
        _levelCamera ??= (LevelCamera)GetTree().GetFirstNodeInGroup("camera");
    }
    public override void _Input(InputEvent @event) {
        if (!IsGodMode) return;
        if (_playerMediator == null) return;
        if (_playerMediator.playerMovement.IsInPipeTransport) return;
        if (@event is not InputEventKey { Pressed: true } keyEvent) return;
        
        // Todo: God Mode 开启检测
        //if (!GameManager.IsGodMode) return; ???

        var pSuit = _playerMediator.playerSuit;
        
        switch (keyEvent.Keycode) {
            case Key.Key1:
                pSuit.Suit = PlayerSuit.SuitEnum.Small;
                pSuit.StarmanOver();
                IsGodFly = false;
                break;
            
            case Key.Key2:
                pSuit.Suit = PlayerSuit.SuitEnum.Super;
                pSuit.StarmanOver();
                IsGodFly = false;
                break;
            
            case Key.Key3:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Fireball;
                pSuit.StarmanOver();
                IsGodFly = false;
                break;
            
            case Key.Key4:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Beetroot;
                pSuit.StarmanOver();
                IsGodFly = false;
                break;
            
            case Key.Key5:
                pSuit.Suit = PlayerSuit.SuitEnum.Powered;
                pSuit.Powerup = PlayerSuit.PowerupEnum.Lui;
                pSuit.StarmanOver();
                IsGodFly = false;
                break;
            
            case Key.Key6:
                if (!pSuit.Starman) {
                    pSuit.Starman = true;
                    pSuit.StarmanTimer = 0;
                } else {
                    pSuit.StarmanOver();
                }
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
                // 在左右各一侧屏外取消飞行状态情况的处理
                if (IsGodFly) return;
                var playerMovement = _playerMediator.playerMovement;
                playerMovement.SpeedX = 0f;
                playerMovement.SpeedY = 0f;
                playerMovement.LastPositionX = _playerMediator.player.Position.X;
                break;
            
            case Key.Key9:
                EmitSignal(SignalName.AddLife);
                break;
            
            case Key.Key0:
                if (_levelCamera == null) {
                    GD.PushError("PlayerGodMode: LevelCamera is null!");
                    break;
                }
                if (_levelCamera.AutoScrollEnded) break;
                if (_levelCamera.CameraMode != LevelCamera.CameraModeEnum.FollowPlayer) {
                    ForceScrollDisabled = !ForceScrollDisabled;
                    _levelCamera.ForceScrollDisabled = ForceScrollDisabled;
                }
                break;
            
            case Key.Pageup:
                _checkpoints = GetTree().GetNodesInGroup("checkpoint");
                if (_checkpoints == null) break;
                if (GameManager.CurrentCheckpointId < _checkpoints.Count) {
                    GameManager.CurrentCheckpointId += 1;
                } else {
                    return;
                }
                foreach (var node in _checkpoints) {
                    if (node is not Checkpoint checkpoint) continue;
                    if (GameManager.CurrentCheckpointId != checkpoint.Id) continue;
                    _playerMediator.player.Position = checkpoint.Position + Vector2.Up * 8f;
                    _playerMediator.player.ForceUpdateTransform();
                    _playerMediator.player.ResetPhysicsInterpolation();
                    break;
                }
                break;
            case Key.Pagedown:
                _checkpoints = GetTree().GetNodesInGroup("checkpoint");
                if (_checkpoints == null) break;
                if (GameManager.CurrentCheckpointId > 1) {
                    GameManager.CurrentCheckpointId -= 1;
                } else {
                    return;
                }
                foreach (var node in _checkpoints) {
                    if (node is not Checkpoint checkpoint) continue;
                    if (GameManager.CurrentCheckpointId != checkpoint.Id) continue;
                    _playerMediator.player.Position = checkpoint.Position + Vector2.Up * 8f;
                    _playerMediator.player.ForceUpdateTransform();
                    _playerMediator.player.ResetPhysicsInterpolation();
                    break;
                }
                break;
        }
    }
    public override void _PhysicsProcess(double delta) {
        IsGodMode = GameManager.IsGodMode;
        if (IsGodInvincible && _playerMediator != null)
            _playerMediator.playerDieAndHurt.HurtInvincibleTimer = 0;
    }
}
