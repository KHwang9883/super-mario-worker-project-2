using Godot;
using System;
using System.IO;
using SMWP.Util;

public partial class ScreenFreeze : CanvasLayer {
    [Signal]
    public delegate void ScreenFrozenEventHandler();

    [Export] private Sprite2D _frozenScreenSprite = null!;
    [Export] private Node2D _loadingSprites = null!;
    [Export] private bool _showLoading;
    [Export] private SmwpFrameTimer _timer = null!;
    [Export] private int _freezeTime = 2;
    
    private Viewport _viewport = null!;
    private Image _frozenScreenImage = new();
    private bool _visibility;
    private bool _freeze;
    private int _freezeTimer;

    public override void _Ready() {
        GetTree().SceneChanged += UpdateViewport;
        _loadingSprites.Visible = _showLoading;
        //RenderingServer.FramePostDraw += DrawFreeze;
    }

    public void UpdateViewport() {
        _viewport = GetTree().Root.GetViewport();
    }
    public override void _Process(double delta) {
        if (_freeze) {
            _freezeTimer++;
            if (_freezeTimer >= _freezeTime) {
                // 异常的屏幕冻结时长（超时）
                _freezeTimer = 0;
                _freeze = false;
            }
            
            if (Visible && _visibility != Visible) {
                _timer.Start();
            }
        } else {
            Visible = false;
            _frozenScreenSprite.Visible = false;
        }
        _visibility = Visible;
    }

    public void OnTimerTimeout() {
        _frozenScreenSprite.Visible = true;
        EmitSignal(SignalName.ScreenFrozen);
    }
    
    public void SetFreeze() {
        _freeze = true;
        _freezeTimer = 0;
        DrawFreeze();
    }
    
    // 画面暂留
    public void DrawFreeze() {
        if (_freeze && !Visible) {
            _viewport = GetTree().Root.GetViewport();
            _frozenScreenImage = _viewport.GetTexture().GetImage();
            _frozenScreenSprite.Texture = ImageTexture.CreateFromImage(_frozenScreenImage);
            var textureSize = _frozenScreenImage.GetSize();
            //GD.Print($"The frozen screen texture size is: {textureSize}");
            _frozenScreenSprite.Scale = Vector2.One / _viewport.GetStretchTransform().Scale;
            Visible = true;
        }
    }
}
