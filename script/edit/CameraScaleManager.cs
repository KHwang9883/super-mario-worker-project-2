using Godot;

public partial class CameraScaleManager : Node {
    [Export] public Button? CameraScaleUpButton;
    [Export] public Button? CameraScaleDownButton;
    [Export] public Camera2D? Camera;

    /// <summary>放大档位，从小到大排列。1.0 必须在数组中，作为默认值。</summary>
    [Export] public float[] ZoomPresets =
        [
            1f / 8f, 1f / 7f, 1f / 6f, 1f / 5f, 1f / 4f, 1f / 3f, 1f / 2f,
            1f,
            1.25f, 1.5f, 2.0f, 3.0f, 4.0f, 8.0f,
        ];

    /// <summary>过渡速度，越大越快。0 表示瞬间切换。</summary>
    [Export] public float LerpSpeed = 40f;

    private int _presetIndex;
    private float _currentZoom = 1f;
    private float _targetZoom = 1f;
    private bool _needsProcess;

    public override void _Ready() {
        base._Ready();
        if (CameraScaleUpButton == null) {
            GD.PushError("CameraScaleUpButton is null");
            return;
        }
        if (CameraScaleDownButton == null) {
            GD.PushError("CameraScaleDownButton is null");
            return;
        }
        CameraScaleUpButton.Pressed += OnScaleUpButtonPressed;
        CameraScaleDownButton.Pressed += OnScaleDownButtonPressed;

        ConfigManager.LoadConfig();
        float savedZoom = (float)ConfigManager.SmwpConfig.GetValue("edit", "camera_zoom", 1.0f);

        _presetIndex = FindClosestPreset(savedZoom);
        _currentZoom = ZoomPresets[_presetIndex];
        _targetZoom = _currentZoom;
        ApplyZoom();
    }

    public override void _Process(double delta) {
        if (!_needsProcess) return;
        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom,
            1f - Mathf.Exp(-LerpSpeed * (float)delta));
        ApplyZoom();
        if (Mathf.Abs(_currentZoom - _targetZoom) < 0.0005f) {
            _currentZoom = _targetZoom;
            ApplyZoom();
            _needsProcess = false;
            SaveZoom();
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

    private void ApplyZoom() {
        if (Camera == null) return;
        Camera.Zoom = new Vector2(_currentZoom, _currentZoom);
    }

    private void SaveZoom() {
        ConfigManager.SmwpConfig.SetValue("edit", "camera_zoom", _currentZoom);
        ConfigManager.SaveConfig();
    }

    public void OnScaleUpButtonPressed() {
        if (Camera == null) return;
        if (_presetIndex < ZoomPresets.Length - 1) {
            _presetIndex++;
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }

    public void OnScaleDownButtonPressed() {
        if (Camera == null) return;
        if (_presetIndex > 0) {
            _presetIndex--;
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }
}
