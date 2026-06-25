using Godot;
using System;

[GlobalClass]
public partial class PanelDrag : Panel {
    private bool _dragging = false;
    private Vector2 _dragOffset = Vector2.Zero;
    private int _originZIndex = 0;
    
    public const float BorderDistance = 64f;

    public override void _Ready() {
        _originZIndex = Math.Max(_originZIndex, ZIndex);
    }

    public override void _EnterTree() {
        base._EnterTree();
        var viewport = GetViewport();
        viewport.SizeChanged += OnViewportSizeChanged;
        BorderLimit();
    }

    public override void _ExitTree() {
        base._ExitTree();
        var viewport = GetViewport();
        viewport.SizeChanged -= OnViewportSizeChanged;
    }

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
                
                // 调整优先级
                ZIndex = _originZIndex + 10;
                //ProcessPriority = -10;
                GetParent().MoveChild(this, GetParent().GetChildCount() - 1);
            }
            else {
                ZIndex = _originZIndex;
                //ProcessPriority = 0;
                _dragging = false;
                AcceptEvent();
            }
        }
        else if (@event is InputEventMouseMotion motionEvent && _dragging) {
            Position = GlobalToParentLocal(GetGlobalMousePosition()) - _dragOffset;
            BorderLimit();
            AcceptEvent();
        }
    }

    public void OnViewportSizeChanged() {
        BorderLimit(true);
    }
    
    public void BorderLimit(bool isViewport = false) {
        Callable.From(() => {
            if (GetParent() is not Control parent)
                return;
            
            float minX = Mathf.Min(0, -Size.X + BorderDistance);
            float maxX = Mathf.Max(0, parent.Size.X - BorderDistance);
            float minY = Mathf.Min(0, -Size.Y + BorderDistance);
            float maxY = Mathf.Max(0, parent.Size.Y - BorderDistance);
            if (isViewport) {
                minX = Mathf.Min(0, parent.Size.X - Size.X);
                maxX = Mathf.Max(0, parent.Size.X - Size.X);
                minY = Mathf.Min(0, parent.Size.Y - Size.Y);
                maxY = Mathf.Max(0, parent.Size.Y - Size.Y);
            }

            Position = new Vector2(
                Mathf.Clamp(Position.X, minX, maxX),
                Mathf.Clamp(Position.Y, minY, maxY)
            );
        }).CallDeferred();
    }
}