using Godot;
using System;

[Tool]
public partial class ViewControl : Node2D {
    [Export] public Rect2 ViewRect;
    
    [Export] private Line2D? _line2D;
    [Export] private Sprite2D? _topLeft;
    [Export] private Sprite2D? _bottomRight;

    private SceneControl? _sceneControl;

    public override void _Ready() {
        Visible = Engine.IsEditorHint();
    }
    public override void _PhysicsProcess(double delta) {
        if (Engine.IsEditorHint()) {
            if (_line2D != null) {
                _line2D.Points = [
                    ViewRect.Position,
                    new Vector2(ViewRect.Position.X + ViewRect.Size.X, ViewRect.Position.Y),
                    ViewRect.Position + ViewRect.Size,
                    new Vector2(ViewRect.Position.X, ViewRect.Position.Y + ViewRect.Size.Y),
                ];
            }
            if (_topLeft != null && _bottomRight != null) {
                _topLeft.GlobalPosition = ViewRect.Position;
                _bottomRight.GlobalPosition = ViewRect.Position + ViewRect.Size;
            }
        }
    }
    
    public void SetSceneControl(SceneControl sceneControl) {
        _sceneControl = sceneControl;
    }
    public void SetLevelScene() {
        // 没有与场景控制元件相连
        if (_sceneControl == null) return;
        _sceneControl.SetSceneStatus();
    }
}
