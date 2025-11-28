using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Player;

public partial class BgmPlayer : AudioStreamPlayer {
    private Node2D? _player;
    private PlayerDieAndHurt? _playerDieAndHurt;
    private PlayerSuit? _playerSuit;
    private LevelConfig? _levelConfig;
    private AudioStreamPlayer? _bgm146Player;
    private bool _fadeOut;

    private float _onWaterVolume;
    private float _underWaterVolume;

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
        
        _bgm146Player = GetNode<AudioStreamPlayer>("Bgm146Player");
    }
    public override void _PhysicsProcess(double delta) {
        // 过关与 Fast Retry 处理
        if (LevelManager.IsLevelPass && Playing) {
            Stop();
        }
        if (_fadeOut) {
            VolumeLinear = Mathf.MoveToward(VolumeLinear, 0.2f, 0.15f);
            if (_bgm146Player != null) _bgm146Player.VolumeLinear = VolumeLinear;
            return;
        }

        // 第 146 号 BGM 音频处理
        if (_bgm146Player == null) return;
        if (_levelConfig is not { BgmId: 146 }) {
            VolumeLinear = Mathf.MoveToward(VolumeLinear, 1f, 0.05f);
            return;
        }
        
        // 子节点音频播放同步
        if (Playing && !_bgm146Player.Playing)
            _bgm146Player.Play(GetPlaybackPosition());
        if (!Playing && _bgm146Player.Playing)
            _bgm146Player.Playing = false;
        
        if (_player == null) {
            // 出于顺序问题在 _Ready() 中延迟引用的 Player 不及时
            _player = (Node2D)GetTree().GetFirstNodeInGroup("player");
            if (_player == null)
                GD.PushError($"{this}: Player is null!");
        } else {
            var playerMovement = (PlayerMovement)_player.GetMeta("PlayerMovement");
            switch (playerMovement.IsAroundWater) {
                case true when !playerMovement.IsInWater:
                    _onWaterVolume = 0.5f;
                    _underWaterVolume = 0.5f;
                    break;
                case false:
                    _onWaterVolume = 1f;
                    _underWaterVolume = 0f;
                    break;
                default: {
                    if (playerMovement.IsInWater) {
                        _onWaterVolume = 0f;
                        _underWaterVolume = 1f;
                    }
                    break;
                }
            }
            VolumeLinear = Mathf.MoveToward(VolumeLinear, _onWaterVolume, 0.1f);
            _bgm146Player.VolumeLinear = Mathf.MoveToward(_bgm146Player.VolumeLinear, _underWaterVolume, 0.1f);
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
