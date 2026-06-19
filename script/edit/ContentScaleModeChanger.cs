using Godot;
using System;

public partial class ContentScaleModeChanger : Node {
    private Window? _window;
    
	public override void _Ready() {
        _window = GetTree().Root;
        bool isMaximized = (_window.Mode == Window.ModeEnum.Maximized);
        _window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        Callable.From(() => {
            if (isMaximized) _window.Mode = Window.ModeEnum.Maximized;
        }).CallDeferred();
	}

	public override void _ExitTree() {
        if (_window == null) return;
        
        _window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
	}
}
