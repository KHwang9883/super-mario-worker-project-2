using Godot;
using System;

public partial class ViewControl : Node2D {
    [Export] public Rect2 ViewRect;

    public override void _Ready() {
        AddToGroup("view_control");
    }
}
