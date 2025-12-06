using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;

public partial class ShaderTest : Node2D {
    private static ColorRect? _colorRect;
    public const int MaxLights = 128;   // 与 Shader 中的 MAX_LIGHTS 保持一致
    private static Vector2[] Positions = new Vector2[MaxLights];
    private static int _positionCount = 0;
    
    public override void _Ready() {
        _colorRect = GetParent().GetNode<ColorRect>("ColorRect");
    }
    public override void _PhysicsProcess(double delta) {
        if (_colorRect == null) {
            _colorRect = GetParent().GetNode<ColorRect>("ColorRect");
            if (_colorRect == null) {
                GD.PushError("ShaderTest: _colorRect is not assigned!");
            }
        } else {
            Positions[_positionCount] = Position;
            _positionCount++;
            if (_positionCount >= MaxLights) {
                _positionCount = 0;
            }
            GD.Print(Positions);
            var shaderMaterial = (ShaderMaterial)_colorRect.Material;
            shaderMaterial.SetShaderParameter("positions", Positions);
        }
    }
}
