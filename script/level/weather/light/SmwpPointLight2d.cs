using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Level.Tool;

public partial class SmwpPointLight2D : Node2D {
    [Export] public bool Enabled;
    public bool Activate;
    private Marker2D? _marker;
    public Vector2 LightPosition;

    public override void _Ready() {
        _marker = GetNode<Marker2D>("LightOffset");
    }
    
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
