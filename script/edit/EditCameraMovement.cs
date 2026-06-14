using Godot;
using System;

public partial class EditCameraMovement : Node {
    [Export] public NodePath PathToCamera = "..";

    public Camera2D? Camera;
    
    const float Speed = 32f;

    public override void _Ready() {
        base._Ready();
        Camera = GetNode<Camera2D>(PathToCamera);
    }
    
    public override void _Process(double delta) {
        base._Process(delta);
        if (Camera == null) return;

        float cameraSpeedX = 0f;
        if (Input.IsActionPressed("ui_right")) { cameraSpeedX = 1f; }
        if (Input.IsActionPressed("ui_left")) { cameraSpeedX = -1f; }
        float cameraSpeedY = 0f;
        if (Input.IsActionPressed("ui_down")) { cameraSpeedY = 1f; }
        if (Input.IsActionPressed("ui_up")) { cameraSpeedY = -1f; }
        var cameraSpeed = new Vector2(cameraSpeedX, cameraSpeedY) * Speed;
        Camera.Translate(cameraSpeed);
    }
}
