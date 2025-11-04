using Godot;
using System;

namespace SMWP.Level.Tool;

public static class ScreenUtils {
    public static Rect2 GetScreenRect(Node node) {
        Viewport viewport = node.GetViewport();
        Rect2 viewportRect = viewport.GetVisibleRect();
        Transform2D canvasTransform = viewport.CanvasTransform;
        Transform2D inverseTransform = canvasTransform.AffineInverse();
        Rect2 result = new Rect2(inverseTransform * viewportRect.Position, viewportRect.Size);
        return result;
    }
}
