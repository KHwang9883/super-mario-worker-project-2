using Godot;
using System;

public partial class AdaptiveControlFill : Node {
    [Export] public NodePath PathToControlNode { get; set; } = "..";
    private Control? _control;
    private Viewport? _viewport;
    private Rect2 _visibleRect;

    public override void _Ready() {
        if (PathToControlNode == null) {
            GD.PrintErr("AdaptiveControlFill: PathToControlNode is not set.");
            return;
        }
        _control = GetNode<Control>(PathToControlNode);
        if (_control == null) {
            GD.PrintErr("AdaptiveControlFill: Target Control node not found.");
            return;
        }

        _control.Ready += OnControlReady;
    }

    private void OnControlReady() {
        if (_control == null) return;
        
        _viewport = _control.GetViewport();
        _visibleRect = _viewport.GetVisibleRect();
        _viewport.SizeChanged += OnViewportSizeChanged;
        OnViewportSizeChanged();
    }

    private void OnViewportSizeChanged() {
        if (_viewport == null || _control == null) return;

        var window = _control.GetWindow();
        // 如果不是 Disabled 模式，则跳过自适应（留给其他缩放机制处理）
        if (window.ContentScaleMode != Window.ContentScaleModeEnum.Disabled) {
            return;
        }

        _visibleRect = _viewport.GetVisibleRect();

        if (_visibleRect.Size.X < 640 || _visibleRect.Size.Y < 480) {
            _control.Position = Vector2.Zero;
            _control.Size = new Vector2(640, 480);

            window.ContentScaleMode = Window.ContentScaleModeEnum.Viewport;
            window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
            window.ContentScaleSize = new Vector2I(640, 480);

            ProjectSettings.SetSetting("display/window/stretch/mode", "visible_rect");
            GD.Print($"[{Time.GetTimeStringFromSystem()}] [GameRoomSize] stretch mode: {ProjectSettings.GetSetting("display/window/stretch/mode")}");
            return;
        }

        var viewportTransform = _viewport.GetCanvasTransform();
        var roomPosition = viewportTransform.AffineInverse() * _visibleRect.Position;
        var roomSize = _visibleRect.Size / viewportTransform.Scale;

        _control.Position = roomPosition;
        _control.Size = roomSize;
    }
}