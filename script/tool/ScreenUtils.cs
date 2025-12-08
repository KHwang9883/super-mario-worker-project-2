using Godot;
using System;

namespace SMWP.Level.Tool;

public static class ScreenUtils {
    public static Rect2 GetScreenRect(Node node) {
        if (!node.IsInsideTree()) {
            //GD.PushError($"Failed to GetScreenRect. {node.Name} is not inside tree!");
            return new Rect2(Vector2.Zero, new Vector2(99999999f, 99999999f));
        }
        // 尝试获取当前场景中的活跃相机
        // 注：此方法是为了防止延迟，目前低海拔 Checkpoint 处复活有用
        // 注2：经测试，Godot 的 Camera2D 在场景开始时改变Position会导致屏幕坐标延迟 3~4 帧更新，因此弃用但暂时保留
        /*Viewport viewportCamera = node.GetViewport();
        Camera2D camera = viewportCamera.GetCamera2D();
    
        if (camera != null && camera.IsInsideTree()) {
            var cameraCenter = camera.GetTargetPosition();
            return new Rect2(
                cameraCenter - new Vector2(320f, 240f), camera.GetViewport().GetVisibleRect().Size
                );
        }*/
    
        // 如果没有活跃相机，回退到原来的方法
        Viewport viewport = node.GetViewport();
        Rect2 viewportRect = viewport.GetVisibleRect();
        Transform2D canvasTransform = viewport.CanvasTransform;
        Transform2D inverseTransform = canvasTransform.AffineInverse();
        Rect2 result = new Rect2(inverseTransform * viewportRect.Position, viewportRect.Size);
        return result;
    }
}
