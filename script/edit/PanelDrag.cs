using Godot;
using System;

[GlobalClass]
public partial class PanelDrag : Panel {
    private bool _dragging = false;
    private Vector2 _dragOffset = Vector2.Zero;

    private Vector2 GlobalToParentLocal(Vector2 globalPos) {
        if (GetParent() is Control parent) {
            return (globalPos - parent.GlobalPosition) / parent.Scale;
        }
        return globalPos;
    }

    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left) {
            if (mouseEvent.Pressed) {
                _dragging = true;
                _dragOffset = GlobalToParentLocal(GetGlobalMousePosition()) - Position;
                AcceptEvent();
            }
            else {
                _dragging = false;
                AcceptEvent();
            }
        }
        else if (@event is InputEventMouseMotion motionEvent && _dragging) {
            Position = GlobalToParentLocal(GetGlobalMousePosition()) - _dragOffset;
            AcceptEvent();
        }
    }
}