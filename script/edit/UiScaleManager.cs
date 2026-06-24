using Godot;
using System.Collections.Generic;

public partial class UiScaleManager : Node {
    [Export] public Button? UiScaleUpButton;
    [Export] public Button? UiScaleDownButton;
    [Export] public Control? UiControl;

    /// <summary>放大档位，从小到大排列。1.0 必须在数组中，作为默认值。</summary>
    [Export] public float[] ZoomPresets =
        [0.5f, 0.75f, 1.0f, 1.125f, 1.25f, 1.5f, 1.75f, 2.0f, 2.5f, 3.0f, 4.0f];

    /// <summary>过渡速度，越大越快。0 表示瞬间切换。</summary>
    [Export] public float LerpSpeed = 36f;

    /// <summary>缩放时保持屏幕位置不变的浮动面板节点（如 PanelDrag）。</summary>
    [Export] public NodePath[]? FloatingPanelPaths;

    private Control[]? _floatingPanels;

    private int _presetIndex;
    private float _currentZoom = 1f;
    private float _targetZoom = 1f;
    private bool _needsProcess;

    private readonly Dictionary<Control, Vector2> _savedPositions = new();

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
        ResolveFloatingPanels();
        _presetIndex = FindClosestPreset(1.0f);

        ConfigManager.LoadConfig();
        float savedZoom = (float)ConfigManager.SmwpConfig.GetValue("edit", "ui_zoom", 1.0f);

        _presetIndex = FindClosestPreset(savedZoom);
        _currentZoom = ZoomPresets[_presetIndex];
        _targetZoom = _currentZoom;
        UpdateButtonStates();
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
            _savedPositions.Clear();
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

    private void OnViewportSizeChanged() {
        _savedPositions.Clear();
        UpdateLayout();
    }

    private void UpdateLayout() {
        if (UiControl == null) return;
        var viewportSize = GetViewport().GetVisibleRect().Size;
        UiControl.Scale = new Vector2(_currentZoom, _currentZoom);
        UiControl.Size = viewportSize / _currentZoom;

        if (_savedPositions.Count > 0) {
            foreach (var kvp in _savedPositions) {
                kvp.Key.Position = kvp.Value / _currentZoom;
            }
        }
    }

    private void ResolveFloatingPanels() {
        if (FloatingPanelPaths == null || FloatingPanelPaths.Length == 0) {
            _floatingPanels = null;
            return;
        }
        _floatingPanels = new Control[FloatingPanelPaths.Length];
        for (int i = 0; i < FloatingPanelPaths.Length; i++) {
            if (FloatingPanelPaths[i] != null) {
                _floatingPanels[i] = GetNodeOrNull<Control>(FloatingPanelPaths[i]);
            }
        }
    }

    private void SaveFloatingPanelPositions() {
        _savedPositions.Clear();
        if (_floatingPanels == null) return;
        foreach (var panel in _floatingPanels) {
            if (panel != null) {
                _savedPositions[panel] = panel.Position * _currentZoom;
            }
        }
    }

    private void UpdateButtonStates() {
        UiScaleUpButton!.Disabled = _presetIndex >= ZoomPresets.Length - 1;
        UiScaleDownButton!.Disabled = _presetIndex <= 0;
    }

    private void SaveZoom() {
        ConfigManager.SmwpConfig.SetValue("edit", "ui_zoom", _currentZoom);
        ConfigManager.SaveConfig();
    }

    public void OnScaleUpButtonPressed() {
        if (UiControl == null) return;
        if (_presetIndex < ZoomPresets.Length - 1) {
            SaveFloatingPanelPositions();
            _presetIndex++;
            UpdateButtonStates();
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }

    public void OnScaleDownButtonPressed() {
        if (UiControl == null) return;
        if (_presetIndex > 0) {
            SaveFloatingPanelPositions();
            _presetIndex--;
            UpdateButtonStates();
            _targetZoom = ZoomPresets[_presetIndex];
            _needsProcess = true;
        }
    }
}
