using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Level.Tool;

public partial class SmwpPointLight2D : Node2D {
    public bool Enabled;
    
    // 发光默认一直禁用，直到入屏启用
    public override void _PhysicsProcess(double delta) {
        Enabled = true;
    }

    public void OnScreenExited() {
        Enabled = false;
    }
}
