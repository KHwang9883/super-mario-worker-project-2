using Godot;
using System;

public partial class Water : Area2D {
    [Export] public Resource waterBoundary { get; private set; } = null!;
    [Export] private Parallax2D _parallaxWater = null!;
    [Export] private AnimatedSprite2D _waterSurfaceSprite = null!;
    [Export] private ColorRect _waterRect = null!;
    [Export] private Camera2D _camera = null!;

    // 测试水块跟随用，可以删除
    //private double _testWaterPhase = 233f;

    public override void _Process(double delta) {
        
        string currentAnim = _waterSurfaceSprite.Animation;
        int currentFrame = _waterSurfaceSprite.Frame;
        
        Texture2D currentTexture = _waterSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
        // 计算水块位置
        _waterRect.GlobalPosition = new Vector2(
            _waterRect.GlobalPosition.X, 
            (float)Math.Max(
                GlobalPosition.Y + currentTexture.GetHeight(), 
                _camera.GetScreenCenterPosition().Y - GetViewportRect().Size.Y * 0.5
            )
        );
        
        // 测试水块跟随用，可以删除
        //Position = new Vector2(Position.X, 233 + (float)Math.Sin(_testWaterPhase) * 128);
        //_testWaterPhase += delta * 2f;
    }
}