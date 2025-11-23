using Godot;
using System;

public partial class SmwpPointLight2d : PointLight2D {
    public void OnScreenEntered() {
        Enabled = true;
        Visible = true;
    }
    public void OnScreenExited() {
        Enabled = false;
        Visible = false;
    }
}
