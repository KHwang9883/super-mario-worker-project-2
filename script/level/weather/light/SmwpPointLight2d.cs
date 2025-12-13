using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Level.Tool;

public partial class SmwpPointLight2D : Node2D {
    private static Node2D? _darknessMask;
    public const int MaxLights = 256;   // 与 Shader 中的 MAX_LIGHTS 保持一致
    private static Vector2[] Positions = new Vector2[MaxLights];
    private static int _positionCount = 0;
    public bool Enabled;
    
    public override void _Ready() {
        _darknessMask = (Node2D)GetTree().GetFirstNodeInGroup("darkness_mask");
        // Debug
        //AddToGroup("lights");
    }
    public override void _PhysicsProcess(double delta) {
        //Enabled = true;
        
        if (_darknessMask == null) {
            _darknessMask = (Node2D)GetTree().GetFirstNodeInGroup("darkness_mask");
            if (_darknessMask == null) {
                GD.PushError("SmwpPointLight2D: _darknessMask is not assigned!");
            }
        } else {
            var screen = ScreenUtils.GetScreenRect(this);
            Positions[_positionCount] = GlobalPosition - screen.Position;
            _positionCount++;
            if (_positionCount >= MaxLights) {
                _positionCount = 0;
            }
            //GD.Print(Positions);
            var shaderMaterial = (ShaderMaterial)_darknessMask.Material;
            shaderMaterial.SetShaderParameter("positions", Positions);
        }
    }
}
