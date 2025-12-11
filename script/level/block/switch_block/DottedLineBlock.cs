using Godot;
using System;
using SMWP.Level;

public partial class DottedLineBlock : StaticBody2D {
    [Export] private AnimatedSprite2D _ani = null!;
    
    public override void _Ready() {
        if (!LevelManager.IsColorAccessibilityMode) return;
        _ani.Play("color-vision");
    }
}
