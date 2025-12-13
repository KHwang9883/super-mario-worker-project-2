using Godot;
using System;
using System.Linq;
using SMWP.Level;

public partial class DarkController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public float DarkLevel;
    
    private LevelConfig? _levelConfig;
    //private CanvasModulate? _canvasModulate;
    private Sprite2D? _darknessMask;
    private static ShaderMaterial? _shaderMaterial;
    private float _thunderDark;

    // Sprite2D + GDShader 方案
    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        //_canvasModulate = (Sprite2D)GetTree().GetFirstNodeInGroup("canvas_modulate");
        _thunderDark = _levelConfig.Darkness / 9f;
        _darknessMask = (Sprite2D)GetTree().GetFirstNodeInGroup("darkness_mask");
        _shaderMaterial = (ShaderMaterial)_darknessMask.Material;
    }
    public override void _Process(double delta) {
        if (_levelConfig == null || _darknessMask == null) return;
        
        DarkLevel = _levelConfig.Darkness;
        var darkValue = Mathf.Min(DarkLevel / 9f, _thunderDark);
        //_darknessMask.Modulate = _darknessMask.Modulate with { A = darkValue };
        _shaderMaterial?.SetShaderParameter("darkness", darkValue);
        _thunderDark = Mathf.MoveToward(_thunderDark, DarkLevel / 9f, 0.06f);
    }
    
    public void OnThunderAppear() {
        _thunderDark = 0f;
    }
    
    // CanvasModulate 方案（OpenGL 渲染模式适应性差，光源易闪烁，弃用）
    /*public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _canvasModulate = (CanvasModulate)GetTree().GetFirstNodeInGroup("canvas_modulate");
        _thunderDark = _levelConfig.Darkness / 9f;
    }
    public override void _Process(double delta) {
        if (_levelConfig == null || _canvasModulate == null) return;
        
        DarkLevel = _levelConfig.Darkness;
        var darkValue = 1f - Mathf.Min(DarkLevel / 9f, _thunderDark);
        _canvasModulate.Color = new Color(darkValue, darkValue, darkValue, 1);
        _thunderDark = Mathf.MoveToward(_thunderDark, DarkLevel / 9f, 0.06f);
    }
    public void OnThunderAppear() {
        _thunderDark = 0f;
    }*/
}
