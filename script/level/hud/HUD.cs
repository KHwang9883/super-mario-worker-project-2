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
    [Export] private Label? _levelInfo;
    
    private bool _timeWarned;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _rock;
    private float _timeHUDShake;
    private Vector2 _timeOriginPosition;
    private Node2D? _player;
    private PlayerGodMode? _godModeNode;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        if (!_levelConfig.HUDDisplay) Visible = false;
        if (_levelInfo == null) return;
        _levelInfo.Text =
            $"Modified Movement: {YesOrNo(_levelConfig.ModifiedMovement)}\n" +
            $"Advanced Switch: {YesOrNo(_levelConfig.AdvancedSwitch)}\n" +
            $"MF Style Beet: {YesOrNo(_levelConfig.MfStyleBeet)}\n" +
            $"Celeste Style Switch: {YesOrNo(_levelConfig.CelesteStyleSwitch)}\n" +
            $"MF Style Pipe Exit: {YesOrNo(_levelConfig.MfStylePipeExit)}";
    }
    public override void _PhysicsProcess(double delta) {
        _godModeNode = (PlayerGodMode)_player!.GetMeta("PlayerGodMode");
        
        if (_life != null) {
            _life.Text = !_godModeNode.IsGodMode ?
                $"MARIO {LevelManager.Life.ToString()}"
                :$"GOD   {LevelManager.Life.ToString()}";
        }
        if (_score != null) _score.Text = LevelManager.Score.ToString();
        
        // Todo: LevelTitle 特殊处理
        if (_levelTitle != null && _levelConfig != null)
            _levelTitle.Text = ConvertHashAndNewline(_levelConfig.LevelTitle);
        
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
        
        // Level Info
        if (_levelInfo != null) _levelInfo.Visible = Input.IsActionPressed("level_info");
        
        // God Mode 摄像机模式坐标显示
        if (_godPosition != null && _player != null) {
            _godPosition.Visible = _godModeNode.IsGodFly;
            _godPosition.Text = $"({_player.Position.X:F2}, {_player.Position.Y:F2})" ;
        }
    }
    
    public string ConvertHashAndNewline(string input) {
        if (string.IsNullOrEmpty(input)) return input;

        const string tempPlaceholder = "☃";
        string step1 = input.Replace(@"\#", tempPlaceholder);
        string step2 = step1.Replace("#", "\n");
        string result = step2.Replace(tempPlaceholder, "#");
        return result;
    }
    public string YesOrNo(bool boolean) {
        return boolean ? "Yes" : "No";
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
