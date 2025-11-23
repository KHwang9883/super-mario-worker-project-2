using Godot;
using System;

public partial class SmwpSpriteLight2d : Sprite2D {
    public void OnScreenEntered() {
        Visible = true;
    }
    public void OnScreenExited() {
        Visible = false;
    }
}
