using Godot;

public partial class UiScaleManager : Node {
    [Export] public Button? UiScaleUpButton;
    [Export] public Button? UiScaleDownButton;
    [Export] public Control? UiControl;

    private float _zoom = 1f;
    private const float ZoomStep = 0.2f;

    public override void _Ready() {
        base._Ready();
        if (UiScaleUpButton == null) {
            GD.PushError("UiScaleUpButton is null");
            return;
        }
        if (UiScaleDownButton == null) {
            GD.PushError("UiScaleDownButton is null");
            return;
        }
        UiScaleUpButton.Pressed += OnScaleUpButtonPressed;
        UiScaleDownButton.Pressed += OnScaleDownButtonPressed;

        GetViewport().SizeChanged += OnViewportSizeChanged;

        if (UiControl != null) {
            UiControl.AnchorRight = 0;
            UiControl.AnchorBottom = 0;
        }
        UpdateLayout();
    }

    private void OnViewportSizeChanged() {
        UpdateLayout();
    }

    /// <summary>
    /// Size = viewport / zoom，原点左上角，Scale 补齐后视觉铺满视口。
    /// </summary>
    private void UpdateLayout() {
        if (UiControl == null) return;
        var viewportSize = GetViewport().GetVisibleRect().Size;
        UiControl.Scale = new Vector2(_zoom, _zoom);
        UiControl.Size = viewportSize / _zoom;
    }

    public void OnScaleUpButtonPressed() {
        if (UiControl == null) return;
        _zoom += ZoomStep;
        UpdateLayout();
    }

    public void OnScaleDownButtonPressed() {
        if (UiControl == null) return;
        _zoom -= ZoomStep;
        UpdateLayout();
    }
}
