using Godot;
using System;
using SMWP.Level;

public partial class MarioworkerChaoJiBug : AnimatedSprite2D {
    private Control _control = null!;
    private LevelConfig? _levelConfig;

    private bool _version;
    private bool _activate;
    private bool _mouseScaled;
    private float _xScale = 1f;

    public override void _Ready() {
        Callable.From(() => {
            _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
            if (_levelConfig.SmwpVersion < 2000) {
                _version = true;
            }
        }).CallDeferred();
        
        _control = GetNode<Control>("../MouseDetect");
        GD.Print(_control);
        _control.MouseEntered += OnMouseEntered;
        _control.MouseExited += OnMouseExited;
    }

    public override void _PhysicsProcess(double delta) {
        if (!_version) {
            return;
        }
        if (_mouseScaled) {
            Callable.From(() => {
                Scale = new Vector2(_xScale, 1f);
            }).CallDeferred();
        }
        
        if (!_activate) {
            return;
        }
        
        if (Input.IsActionJustPressed("mouse_wheel_up")) {
            _xScale = Mathf.Clamp(_xScale + 5f, 1f, 64f);
            _mouseScaled = true;
            //GD.Print("Mouse Wheel Up");
        }
        else if (Input.IsActionJustPressed("mouse_wheel_down")) {
            _xScale = Mathf.Clamp(_xScale - 5f, 1f, 64f);
            _mouseScaled = true;
            //GD.Print("Mouse Wheel Down");
        }
    }

    public void OnMouseEntered() {
        _activate = true;
        //GD.Print("Marioworker超级Bug: Activated");
    }

    public void OnMouseExited() {
        _activate = false;
        //GD.Print("Marioworker超级Bug: Inactivated");
    }
}
