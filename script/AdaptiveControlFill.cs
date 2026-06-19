using Godot;
using System;

public partial class AdaptiveControlFill : Node {
    [Export] public NodePath PathToControlNode { get; set; } = "..";
    private Control? _control;
    private Viewport? _viewport;
    private Rect2 _visibleRect;
    
    private static readonly Vector2I DefaultResolution = new Vector2I(640, 480);

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
            _control.Size = DefaultResolution;
            return;
        }

        _visibleRect = _viewport.GetVisibleRect();
        _control.Size = _visibleRect.Size;
    }
}