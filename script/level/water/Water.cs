using Godot;
using System;

public partial class Water : Area2D {
    [Export] private Parallax2D _parallaxWater = null!;
    [Export] private AnimatedSprite2D _waterSurfaceSprite = null!;
    [Export] private ColorRect _waterRect = null!;
    [Export] private Camera2D _camera = null!;

    private double _testWaterPhase = 233f;

    public override void _Process(double delta) {
        // 获取当前动画名称和当前帧索引
        string currentAnim = _waterSurfaceSprite.Animation;
        int currentFrame = _waterSurfaceSprite.Frame;
        
        // 使用当前动画和帧索引获取纹理
        Texture2D currentTexture = _waterSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
        // 计算水面位置
        _waterRect.GlobalPosition = new Vector2(
            _waterRect.GlobalPosition.X, 
            (float)Math.Max(
                GlobalPosition.Y + currentTexture.GetHeight(), 
                _camera.GetScreenCenterPosition().Y - GetViewportRect().Size.Y * 0.5
            )
        );
        
        // 水面波动效果
        Position = new Vector2(Position.X, 233 + (float)Math.Sin(_testWaterPhase) * 128);
        _testWaterPhase += delta * 2f;
    }
}