using Godot;
using System;

public partial class CursorSpriteManager : Node {
	[Export] public Texture2D Normal = null!;
	[Export] public Texture2D Imitator = null!;

    public override void _Process(double delta) {
        base._Process(delta);

        // TODO: 检测模仿者状态
		// 测试模仿者鼠标图像
		if (Input.IsActionJustPressed("move_jump")) {
			DisplayServer.CursorSetCustomImage(Imitator);
		}
		if (Input.IsActionJustPressed("move_fire")) {
			DisplayServer.CursorSetCustomImage(Normal);
		}
	}
}
