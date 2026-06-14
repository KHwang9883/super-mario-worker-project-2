using Godot;

public partial class FitCanvasSize : Control {
    private CanvasLayer? _targetLayer;

    public override void _Ready() {
        _targetLayer = GetParentCanvasLayer();
    }

    public override void _Process(double delta) {
        if (_targetLayer == null) return;
        Vector2 layerScale = _targetLayer.Scale;
        Scale = new Vector2(1.0f / layerScale.X, 1.0f / layerScale.Y);
    }

    private CanvasLayer GetParentCanvasLayer() {
        Node parent = GetParent();
        while (parent != null) {
            if (parent is CanvasLayer layer) return layer;
            parent = parent.GetParent();
        }
        return null;
    }
}