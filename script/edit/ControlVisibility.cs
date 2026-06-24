using Godot;
using System;

[GlobalClass]
public partial class ControlVisibility : Node {
    [Export] public NodePath ToParent = "..";
    
    private Control _parent = null!;

    public override void _Ready() {
        _parent = GetNode<Control>(ToParent);
    }
    public void MakeVisible() {
        _parent.Visible = true;
    }
    public void MakeInvisible() {
        _parent.Visible = false;
    }
    public void ToggleVisibility() {
        _parent.Visible = !_parent.Visible;
    }
}
