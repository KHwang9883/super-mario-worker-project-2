using Godot;
using System;
using SMWP.Level.Score;
using SMWP.Util;

namespace SMWP.Level.HUD;

public partial class HUD : Control {
    [Signal]
    public delegate void PlaySoundTimeWarningEventHandler();
    [Export] private Label _life = null!;
    [Export] private Label _score = null!;
    [Export] private Label _levelTitle = null!;
    [Export] private Control _timeHUD = null!;
    [Export] private Label _timeCounter = null!;
    [Export] private Label _coin = null!;
    [Export] private Sprite2D _gameOverSprite = null!;
    [Export] private Label _godPosition = null!;
    [Export] private Label _levelInfo = null!;
    [Export] private Label _scrollDisabled = null!;
    
    private Node2D? _player;
    private PlayerGodMode? _godModeNode;
    private LevelConfig? _levelConfig;
    
    private bool _timeWarned;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _rock;
    private float _timeHUDShake;
    private Vector2 _timeOriginPosition;
    private string? _smwpGameWindowTitle;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        if (!_levelConfig.HUDDisplay) Visible = false;
        _levelInfo.Text =
            $"Modified Movement: {YesOrNo(_levelConfig.ModifiedMovement)}\n" +
            $"Advanced Switch: {YesOrNo(_levelConfig.AdvancedSwitch)}\n" +
            $"MF Style Beet: {YesOrNo(_levelConfig.MfStyleBeet)}\n" +
            $"Celeste Style Switch: {YesOrNo(_levelConfig.CelesteStyleSwitch)}\n" +
            $"MF Style Pipe Exit: {YesOrNo(_levelConfig.MfStylePipeExit)}";
        _smwpGameWindowTitle = GetTree().Root.GetWindow().Title;
    }
    public override void _PhysicsProcess(double delta) {
        _godModeNode = (PlayerGodMode)_player!.GetMeta("PlayerGodMode");
        
        _life.Text = !_godModeNode.IsGodMode ?
            $"MARIO {GameManager.Life.ToString()}"
            :$"GOD   {GameManager.Life.ToString()}";
        
        _score.Text = GameManager.Score.ToString();
        
        // LevelTitle 特殊处理
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            _levelTitle.Text = StringProcess.ConvertHashAndNewline(_levelConfig.LevelTitle);
            // 版本号小于 1712 则自动加上 WORLD 标题
            //GD.Print($"SmwpVersion: {_levelConfig.SmwpVersion}");
            if (_levelConfig.SmwpVersion < 1712) _levelTitle.Text = "WORLD\n" + _levelTitle.Text;
        }
        
        // 负数时间不显示
        if (GameManager.Time < 0) _timeHUD.Visible = false;
            
        // 时钟警告！
        _timeCounter.Text = GameManager.Time.ToString();
        if (GameManager.Time < 100 && GameManager.Time > 0 && !GameManager.IsLevelPass) {
            OnTimeWarning();
        }
        _timeHUDShake = _rng.RandfRange(0f, _rock) - _rng.RandfRange(0f, _rock);
        _timeHUD.Position = new Vector2(_timeOriginPosition.X + _timeHUDShake, _timeOriginPosition.Y + _timeHUDShake);
        _rock = Mathf.Clamp(_rock - 0.1f, 0f, _rock);

        // 金币
        _coin.Text = GameManager.Coin.ToString();
        
        // Game Over 展示
        if (GameManager.IsGameOver) {
            GameOverShow();
        }
        
        // Level Info
        _levelInfo.Visible = Input.IsActionPressed("level_info");
        if (Input.IsActionJustPressed("level_info")) {
            // Todo: 待测试
            DisplayServer.WindowSetTitle($"[Level Author]: {_levelConfig?.LevelAuthor} ({_levelConfig?.SmwpVersion})");
        } 
        if (Input.IsActionJustReleased("level_info")) {
            DisplayServer.WindowSetTitle(_smwpGameWindowTitle);
        }
        
        // God Mode 摄像机模式坐标显示
        if (_player != null) {
            _godPosition.Visible = _godModeNode.IsGodFly;
            _godPosition.Text = $"({_player.Position.X:F2}, {_player.Position.Y:F2})" ;
        }
        
        // God Mode 强制滚屏禁用
        _scrollDisabled.Visible = _godModeNode.ForceScrollDisabled;
    }
    
    public static string YesOrNo(bool boolean) {
        return boolean ? "Yes" : "No";
    }
    public void OnTimeWarning() {
        if (_timeWarned) return;
        _timeWarned = true;
        EmitSignal(SignalName.PlaySoundTimeWarning);
        _rock = 10f;
    }
    public void GameOverShow() {
        _gameOverSprite.Visible = true;
    }
}
