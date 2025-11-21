using Godot;
using System;
using SMWP.Level.Score;

namespace SMWP.Level.HUD;

public partial class HUD : Control {
    [Signal]
    public delegate void PlaySoundTimeWarningEventHandler();
    [Export] private Label? _life;
    [Export] private Label? _score;
    [Export] private Label? _levelTitle;
    [Export] private Control? _timeHUD;
    [Export] private Label? _timeCounter;
    [Export] private Label? _coin;
    [Export] private Sprite2D? _gameOverSprite;
    [Export] private Label? _godPosition;

    private bool _timeWarned;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _rock;
    private float _timeHUDShake;
    private Vector2 _timeOriginPosition;
    private Node2D? _player;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
    }
    public override void _PhysicsProcess(double delta) {
        if (_life != null) _life.Text = $"MARIO {LevelManager.Life.ToString()}";
        if (_score != null) _score.Text = LevelManager.Score.ToString();
        
        // Todo: LevelTitle 特殊处理
        if (_levelTitle != null) _levelTitle.Text = LevelManager.LevelTitle;
        
        // 负数时间不显示
        if (_timeHUD != null && LevelManager.Time < 0) _timeHUD.Visible = false;
            
        // 时钟警告！
        if (_timeHUD != null) {
            if (_timeCounter != null) _timeCounter.Text = LevelManager.Time.ToString();
            if (LevelManager.Time < 100 && LevelManager.Time > 0 && !LevelManager.IsLevelPass) {
                OnTimeWarning();
            }
            _timeHUDShake = _rng.RandfRange(0f, _rock) - _rng.RandfRange(0f, _rock);
            _timeHUD.Position = new Vector2(_timeOriginPosition.X + _timeHUDShake, _timeOriginPosition.Y + _timeHUDShake);
            _rock = Mathf.Clamp(_rock - 0.1f, 0f, _rock);
        }

        // 金币
        if (_coin != null) _coin.Text = LevelManager.Coin.ToString();
        
        // Game Over 展示
        if (LevelManager.IsGameOver) {
            GameOverShow();
        }
        
        // God Mode 摄像机模式坐标显示
        if (_godPosition != null && _player != null) {
            var godModeNode = (PlayerGodMode)_player.GetMeta("PlayerGodMode");
            _godPosition.Visible = godModeNode.IsGodFly;
            _godPosition.Text = $"({_player.Position.X:F2}, {_player.Position.Y:F2})" ;
        }
    }
    public void OnTimeWarning() {
        if (_timeWarned) return;
        _timeWarned = true;
        EmitSignal(SignalName.PlaySoundTimeWarning);
        _rock = 10f;
    }
    public void GameOverShow() {
        if (_gameOverSprite == null) return;
        _gameOverSprite.Visible = true;
    }
}
