using Godot;
using System;
using System.IO;

public partial class ScreenFreeze : CanvasLayer {
    [Signal]
    public delegate void ScreenFrozenEventHandler();

    [Export] private Sprite2D _frozenScreenSprite = null!;
    
    private Image _frozenScreenImage = new();
    private Vector2 _resolution = new Vector2(640f, 480f);
    private bool _visibility;

    public override void _Ready() {
        RenderingServer.FramePostDraw += DrawFreeze;
    }
    public override void _Process(double delta) {
        if (Visible && _visibility != Visible) {
            _frozenScreenSprite.Texture = ImageTexture.CreateFromImage(_frozenScreenImage);
            /*_frozenScreenSprite.Scale = new Vector2(640f, 480f) / _resolution;
            GD.Print(_resolution);*/
            EmitSignal(SignalName.ScreenFrozen);
        }
        _visibility = Visible;
    }

    public void DrawFreeze() {
        // 画面暂留
        
        // Todo: test button
        if (Input.IsActionJustPressed("confirm")) {
            var viewport = GetTree().Root.GetViewport();
            var viewportTexture = viewport.GetTexture();
            _resolution = viewportTexture.GetSize();
            _frozenScreenImage = viewportTexture.GetImage();
            Visible = true;
        }
    }

    // 异常的屏幕冻结时长（超时）
    public void SetInvisible() {
        Visible = false;
    }
}
