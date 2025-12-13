using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class BrightController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] public int BrightLevel;
    
    private LevelConfig? _levelConfig;
    private static Node2D? _darknessMask;
    private static ShaderMaterial? _shaderMaterial;
    public const int MaxLights = 256;   // 重要：与 Shader 中的 MAX_LIGHTS 保持一致
    private static Vector2[] Positions = new Vector2[MaxLights];
    private static int _positionCount = 0;
    
    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _darknessMask = (Node2D)GetTree().GetFirstNodeInGroup("darkness_mask");
        _shaderMaterial = (ShaderMaterial)_darknessMask.Material;
    }
    public override void _Process(double delta) {
        if (_levelConfig == null) return;
        BrightLevel = _levelConfig.Brightness;
        if (_darknessMask == null) {
            GD.PushError("BrightController: _darknessMask is not assigned!");
            return;
        }
        
        var screen = ScreenUtils.GetScreenRect(this);
        var nodes = GetTree().GetNodesInGroup("lights");

        _positionCount = 0;
        Positions = new Vector2[MaxLights];
        foreach (var node in nodes) {
            if (node is not SmwpPointLight2D light) continue;
            if (!light.Enabled) continue;
            
            Positions[_positionCount] = light.GlobalPosition - screen.Position;
            _positionCount++;
            // 大于最大光源数就停止遍历
            if (_positionCount < MaxLights) continue;
            _positionCount = 0;
            break;
        }
        
        // Apply
        _shaderMaterial?.SetShaderParameter("positions", Positions);
    }
}
