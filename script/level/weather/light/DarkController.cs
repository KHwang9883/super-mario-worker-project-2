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
    private float _thunderDark;

    // Sprite2D + GDShader 方案
    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _darknessMask = (Sprite2D)GetTree().GetFirstNodeInGroup("canvas_modulate");
        _thunderDark = _levelConfig.Darkness / 9f;
    }
    public override void _Process(double delta) {
        if (_levelConfig == null || _darknessMask == null) return;
        
        DarkLevel = _levelConfig.Darkness;
        var darkValue = 1f - Mathf.Min(DarkLevel / 9f, _thunderDark);
        _darknessMask.Modulate = _darknessMask.Modulate with { A = darkValue };
        _thunderDark = Mathf.MoveToward(_thunderDark, DarkLevel / 9f, 0.06f);

        // 发光默认一直禁用，直到入屏启用
        /*var lights = GetTree().GetNodesInGroup("lights");

        // Debug
        GD.Print($"Lights count: {lights.Count(p => p is SmwpPointLight2D { Enabled : true})}");

        foreach (var node in lights) {
            if (node is not SmwpPointLight2D light) continue;
            light.Enabled = false;
        }*/
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
