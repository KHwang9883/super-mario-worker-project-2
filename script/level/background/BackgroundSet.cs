using Godot;
using System;

namespace SMWP.Level.Background;

[GlobalClass]
public partial class BackgroundSet : Node2D {
    [Export] private Sprite2D? _gradient;
    [Export] private Sprite2D? _backgroundBottom;

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
    }
}
