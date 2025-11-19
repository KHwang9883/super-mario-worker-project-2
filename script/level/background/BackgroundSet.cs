using Godot;
using System;
using SMWP.Level.Tool;

namespace SMWP.Level.Background;

[GlobalClass]
public partial class BackgroundSet : Node2D {
    [Export] private Sprite2D? _gradient;
    [Export] private Node2D? _backgroundBottom;
    [Export] private Node2D? _cloudTop;

    private Node2D? _waterSurface;

    public override void _Ready() {
        var levelConfig = (LevelConfig)GetTree().GetFirstNodeInGroup("level_config");
        var width = levelConfig.RoomWidth;
        var height = levelConfig.RoomHeight;
        
        // 渐变色背景
        if (_gradient != null) {
            var gradientTexture2D = (GradientTexture2D)_gradient.Texture;
            gradientTexture2D.Height = (int)height;
            _gradient.Position = new Vector2(_gradient.Position.X, height / 2);
        }

        // 底部背景
        if (_backgroundBottom != null) {
            _backgroundBottom.Position = new Vector2(_backgroundBottom.Position.X, height);
        }
        
        // 顶部云层入水隐藏
        _waterSurface ??= (Node2D)GetTree().GetFirstNodeInGroup("water_global");
    }

    public override void _PhysicsProcess(double delta) {
        if (_cloudTop == null || _waterSurface == null) return;
        
        // 顶部云层入水隐藏
        var screen = ScreenUtils.GetScreenRect(this);
        var modulate = _cloudTop.Modulate;
        if (screen.Position.Y + 100f > _waterSurface.Position.Y) {
            _cloudTop.Modulate = modulate with { A = Mathf.MoveToward(modulate.A, 0f, 0.05f) };
        } else {
            _cloudTop.Modulate = modulate with { A = Mathf.MoveToward(modulate.A, 1f, 0.05f) };
        }
    }
}
