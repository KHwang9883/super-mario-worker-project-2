using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Util;

public partial class SmwpPointLight2D : Node2D {
    [Export] private Marker2D _marker = null!;
    [Export] public bool Enabled;
    public bool Activate;
    public Vector2 LightPosition;
    public float LightRadius = 1f;
    
    // 发光默认一直禁用，直到入屏启用
    public override void _Process(double delta) {
        if (!Enabled) return;
        Activate = true;
        LightPosition = _marker!.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta) {
        // 假装自己动起来，这样就可以被物理插值了
        /*if (!Enabled) return;
        var targetGlobalPosition = GetParent<Node2D>().GlobalPosition + Vector2.Zero;
        GlobalPosition = targetGlobalPosition;*/
    }

    public void OnScreenExited() {
        if (!Enabled) return;
        Activate = false;
    }
}
