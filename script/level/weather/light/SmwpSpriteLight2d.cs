using Godot;
using System;

public partial class SmwpSpriteLight2d : Sprite2D {
    private BrightController? _brightController;
    private Vector2 _originScale;
    private float _scaleRatio;
    public override void _Ready() {
        _brightController = (BrightController)GetTree().GetFirstNodeInGroup("bright_controller");
        _originScale = Scale;
        Visible = true;
    }
    public override void _Process(double delta) {
        if (_brightController == null) return;
        
        switch (_brightController.BrightLevel) {
            case 0:
                _scaleRatio = 0f;
                break;
            case 1:
                _scaleRatio = 0.25f;
                break;
            case 2:
                _scaleRatio = 0.5f;
                break;
            case 3:
                _scaleRatio = 1f;
                break;
            case 4:
                _scaleRatio = 1.8f;
                break;
            case 5:
                _scaleRatio = 3f;
                break;
        }
        Scale = _originScale * _scaleRatio;
        //var shaderMaterial = (ShaderMaterial)Material;
        //shaderMaterial.SetShaderParameter("scale", _scaleRatio);
    }
    public void OnScreenEntered() {
        Visible = true;
    }
    public void OnScreenExited() {
        Visible = false;
    }
}
