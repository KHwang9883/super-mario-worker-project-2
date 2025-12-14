using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Level.Tool;

public partial class SmwpPointLight2D : Node2D {
    public bool Enabled;
    
    // 发光默认一直禁用，直到入屏启用
    public override void _Process(double delta) {
        Enabled = true;
    }

    public override void _PhysicsProcess(double delta) {
        // 假装自己动起来，这样就可以被物理插值了
        /*var targetGlobalPosition = GetParent<Node2D>().GlobalPosition + Vector2.Zero;
        GlobalPosition = targetGlobalPosition;*/
    }

    public void OnScreenExited() {
        Enabled = false;
    }
}
