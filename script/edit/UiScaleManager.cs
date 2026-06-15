using Godot;

public partial class UiScaleManager : Node {
    [Export] public Button? UiScaleUpButton;
    [Export] public Button? UiScaleDownButton;
    [Export] public Control? UiControl;

    /// <summary>放大档位，从小到大排列。1.0 必须在数组中，作为默认值。</summary>
    [Export] public float[] ZoomPresets =
        [0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f, 3.0f, 5.0f];

    /// <summary>过渡速度，越大越快。0 表示瞬间切换。</summary>
    [Export] public float LerpSpeed = 36f;

    private int _presetIndex;
    private float _currentZoom = 1f;
    private float _targetZoom = 1f;
    private bool _needsProcess;

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
        _presetIndex = FindClosestPreset(1.0f);
        _currentZoom = ZoomPresets[_presetIndex];
        _targetZoom = _currentZoom;
        UpdateLayout();
    }

    public override void _Process(double delta) {
        if (!_needsProcess) return;
        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom,
            1f - Mathf.Exp(-LerpSpeed * (float)delta));
        UpdateLayout();
        if (Mathf.Abs(_currentZoom - _targetZoom) < 0.0005f) {
            _currentZoom = _targetZoom;
            UpdateLayout();
            _needsProcess = false;
        }
    }

    private int FindClosestPreset(float target) {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < ZoomPresets.Length; i++) {
            float dist = Mathf.Abs(ZoomPresets[i] - target);
            if (dist < bestDist) {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    private void OnViewportSizeChanged() {
        UpdateLayout();
    }

    private void UpdateLayout() {
        if (UiControl == null) return;
        var viewportSize = GetViewport().GetVisibleRect().Size;
        UiControl.Scale = new Vector2(_currentZoom, _currentZoom);
        UiControl.Size = viewportSize / _currentZoom;
    }

    public void OnScaleUpButtonPressed() {
        if (UiControl == null) return;
        if (_presetIndex < ZoomPresets.Length - 1) {
            _presetIndex++;
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }

    public void OnScaleDownButtonPressed() {
        if (UiControl == null) return;
        if (_presetIndex > 0) {
            _presetIndex--;
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }
}
