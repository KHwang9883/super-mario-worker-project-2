using Godot;
using System;

public partial class Wind : Parallax2D {
    public float SpeedX = -300f;
    private float _targetSpeedX;
    private bool _visibility;
    private WindyController? _windyController;

    public override void _Ready() {
        _windyController = (WindyController)GetTree().GetFirstNodeInGroup("windy_controller");
        Modulate = Modulate with { A = (_windyController.WindyLevel == 0) ? 0f : 1f };
        Visible = true;
    }
    public override void _PhysicsProcess(double delta) {
        if (_windyController == null) return;
        
        _visibility = true;
        switch (_windyController.WindyLevel) {
            case 0:
                _targetSpeedX = 0f;
                _visibility = false;
                break;
            case 1:
                _targetSpeedX = -150f;
                break;
            case 2:
                _targetSpeedX = -400f;
                break;
            case 3:
                _targetSpeedX = -900f;
                break;
        }
        
        Modulate = Modulate with { A = Mathf.MoveToward(Modulate.A, (_visibility) ? 1f : 0f, 0.05f) };
        
        SpeedX = Mathf.MoveToward(SpeedX, _targetSpeedX, 40f);
        Autoscroll = Autoscroll with { X = SpeedX };
    }
}
