using Godot;
using System;

public partial class SmwpPointLight2d : PointLight2D {
    /*private BrightController? _brightController;
    private Vector2 _originScale;
    public override void _Ready() {
        _brightController = (BrightController)GetTree().GetFirstNodeInGroup("bright_controller");
        _originScale = Scale;
        Visible = true;
    }
    public override void _Process(double delta) {
        if (_brightController == null) return;
        
        switch (_brightController.BrightLevel) {
            case 0:
                Scale = _originScale * 0;
                break;
            case 1:
                Scale = _originScale * 0.25f;
                break;
            case 2:
                Scale = _originScale * 0.5f;
                break;
            case 3:
                Scale = _originScale;
                break;
            case 4:
                Scale = _originScale * 1.8f;
                break;
            case 5:
                Scale = _originScale * 3f;
                break;
        }
    }
    public void OnScreenEntered() {
        Enabled = true;
        //Visible = true;
    }
    public void OnScreenExited() {
        Enabled = false;
        //Visible = false;
    }*/
}
