using Godot;
using System;
using SMWP.Edit.Command;

public partial class 草稿 : Node {
	// 点击鼠标放置物品
	public override void _Input(InputEvent @event) {
		base._Input(@event);
		if (@event is InputEventMouseButton mouseEvent) {
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed) {
				var position = GetViewport().GetMousePosition();
				var cmdPlaceObject = new CmdPlaceObject();
				AddChild(cmdPlaceObject);
			}
		}
	}
}
