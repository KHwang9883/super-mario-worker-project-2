using Godot;
using System;
using SMWP.Level.Tool;

public partial class Water : Area2D {
    [Export] public Resource WaterBoundary { get; private set; } = null!;
    [Export] private Parallax2D _parallaxWater = null!;
    [Export] private AnimatedSprite2D _waterSurfaceSprite = null!;
    [Export] private ColorRect _waterRect = null!;

    // 测试水块跟随用，可以删除
    //private double _testWaterPhase = 233f;
    
    public override void _PhysicsProcess(double delta) {
        
        string currentAnim = _waterSurfaceSprite.Animation;
        int currentFrame = _waterSurfaceSprite.Frame;
        
        Texture2D currentTexture = _waterSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
        // 计算水块位置
        var screen = ScreenUtils.GetScreenRect(this);

        //Callable.From(() => {
            _waterRect.GlobalPosition = new Vector2(
                screen.Position.X - 16f,
                Mathf.Max(
                    GlobalPosition.Y + currentTexture.GetHeight(),
                    screen.Position.Y - 16f
                )
            );
        //}).CallDeferred();

        // 测试水块跟随用，可以删除
        //Position = new Vector2(Position.X, 0f + (float)Math.Sin(_testWaterPhase) * 128);
        //_testWaterPhase += delta * 2f;
    }
}