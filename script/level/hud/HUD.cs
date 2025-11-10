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

    private bool _timeWarned;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _rock;
    private float _timeHUDShake;
    private Vector2 _timeOriginPosition;
    
    public override void _PhysicsProcess(double delta) {
        if (_life != null) _life.Text = $"MARIO {LevelManager.Life.ToString()}";
        if (_score != null) _score.Text = LevelManager.Score.ToString();
        
        // Todo: LevelTitle 特殊处理
        if (_levelTitle != null) _levelTitle.Text = LevelManager.LevelTitle;
        
        // 时钟警告！
        if (_timeHUD != null) {
            if (_timeCounter != null) _timeCounter.Text = LevelManager.Time.ToString();
            if (LevelManager.Time < 100 && LevelManager.Time > 0) {
                OnTimeWarning();
            }
            _timeHUDShake = _rng.RandfRange(0f, _rock) - _rng.RandfRange(0f, _rock);
            _timeHUD.Position = new Vector2(_timeOriginPosition.X + _timeHUDShake, _timeOriginPosition.Y + _timeHUDShake);
            _rock = Mathf.Clamp(_rock - 0.1f, 0f, _rock);
        }

        if (_coin != null) _coin.Text = LevelManager.Coin.ToString();
    }

    public void OnTimeWarning() {
        if (_timeWarned) return;
        _timeWarned = true;
        EmitSignal(SignalName.PlaySoundTimeWarning);
        _rock = 10f;
    }
}
