using Godot;
using System;

public partial class SelectLightMask : ColorRect {
    private float _phase;

    public override void _Ready() {
        Visible = false;
    }
    public override void _PhysicsProcess(double delta) {
        if (!Visible) return;
        
        _phase = Mathf.Wrap(_phase + 0.1f, 0f, Mathf.Tau);
        Modulate = Modulate with { A = 0.3f + Mathf.Sin(_phase) / 10f };
    }
    public void OnDisplay() {
        // ProcessMode.Disabled 的情形
        if (!CanProcess()) return;
        Visible = true;
    }
    public void OnHide() {
        // ProcessMode.Disabled 的情形
        if (!CanProcess()) return;
        Visible = false;
    }
}
