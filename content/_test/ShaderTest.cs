using Godot;
using System;

public partial class ShaderTest : Sprite2D {
    private ColorRect _colorRect;

    public override void _Ready() {
        _colorRect = GetParent().GetNode<ColorRect>("ColorRect");
    }

    public override void _Process(double delta) {
        var shaderMaterial = (ShaderMaterial)_colorRect.Material;
        shaderMaterial.SetShaderParameter("light", Texture);
    }
}
