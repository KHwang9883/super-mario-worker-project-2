using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Player;

public partial class BgmPlayer : AudioStreamPlayer {
    private Node2D? _player;
    private PlayerDieAndHurt? _playerDieAndHurt;
    private PlayerSuit? _playerSuit;
    private LevelConfig? _levelConfig;
    private bool _fadeOut;

    public override void _Ready() {
        Callable.From(() => {
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            _playerDieAndHurt = (PlayerDieAndHurt)_player.GetMeta("PlayerDieAndHurt");
            _playerDieAndHurt.PlayerDiedSucceeded += OnPlayerDied;
            _playerSuit = (PlayerSuit)_player.GetMeta("PlayerSuit");
            _playerSuit.PlayerStarmanStarted += OnPlayerStarmanStart;
            _playerSuit.PlayerStarmanFinished += OnPlayerStarmanEnd;
            
            _levelConfig = LevelConfigAccess.GetLevelConfig(this);
            // Fast Retry 读取 BGM 位置
            Play(_levelConfig.FastRetry ? LevelManager.BgmPosition : 0f);
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (LevelManager.IsLevelPass && Playing) {
            Stop();
        }
        if (_fadeOut) {
            VolumeLinear = Mathf.MoveToward(VolumeLinear, 0.2f, 0.15f);
        }
    }
    public void OnPlayerDied() {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null");
        } else {
            if (!_levelConfig.FastRetry) {
                Stop();
            } else {
                _fadeOut = true;
            }
        }
    }
    public void OnPlayerStarmanStart() {
        Stop();
    }
    public void OnPlayerStarmanEnd() {
        Play();
    }
    public void OnTreeExiting() {
        // Fast Retry 记录 BGM 位置
        if (_levelConfig is not LevelConfig { FastRetry: true }) return;
        LevelManager.BgmPosition = GetPlaybackPosition();
        //GD.Print(LevelManager.BgmPosition);
    }
}
