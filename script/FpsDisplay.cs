using Godot;
using System;
using SMWP;

public partial class FpsDisplay : CanvasLayer {
    [Export] private Label? _label;
    [Export] private double _inquiryTime = 0.500;
    
    private double _timer;
    
    public override void _Process(double delta) {
        Visible = GameManager.ShowFps;
        if (!Visible) {
            return;
        }
        _timer += delta;
        if (!(_timer >= _inquiryTime)) {
            return;
        }
        _label?.SetText($"FPS: {Engine.GetFramesPerSecond()}");
        _timer -=  _inquiryTime;
    }
}
