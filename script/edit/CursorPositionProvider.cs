using Godot;
using System;

public static class CursorPositionProvider {
    public static Vector2 GetCursorPosition(Node contextNode) {
        // TODO: Get cursor position
        var viewport = contextNode.GetViewport();
        var mousePosition = viewport.GetMousePosition();
        var camera = viewport.GetCamera2D();
        var realPosition = mousePosition - viewport.CanvasTransform.Origin;
        GD.Print(mousePosition);
        return realPosition;
    }
}
