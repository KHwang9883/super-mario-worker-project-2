using Godot;
using System;

public partial class ContentScaleModeChanger : Node {
	public override void _Ready() {
		GetTree().Root.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
	}

	public override void _ExitTree() {
		GetTree().Root.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
	}
}
