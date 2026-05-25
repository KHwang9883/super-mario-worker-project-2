using Godot;
using System;

public partial class CursorPosition : Node {
	private NodePath _pathToCursor = "..";
	private Node2D _cursor = null!;

	public override void _Ready() {
		_cursor = GetNode<Node2D>(_pathToCursor);
	}

    public override void _Process(double delta) {
        base._Process(delta);
        Callable.From(() => {
            _cursor.GlobalPosition = GetGlobalMousePosition(); 
        }).CallDeferred();
    }

	public Vector2 GetGlobalMousePosition() {
		return GetViewport().GetMousePosition();
	}
}
