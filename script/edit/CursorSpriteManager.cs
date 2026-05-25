using Godot;
using System;

public partial class CursorSpriteManager : Node {
	[Export] public Texture2D Normal = null!;
	[Export] public Texture2D Imitator = null!;

    public override void _Process(double delta) {
        base._Process(delta);

		// 测试模仿者鼠标图像
		if (Input.IsActionJustPressed("move_jump")) {
			DisplayServer.CursorSetCustomImage(Imitator);
		}
		if (Input.IsActionJustReleased("move_fire")) {
			DisplayServer.CursorSetCustomImage(Normal);
		}
	}
}
