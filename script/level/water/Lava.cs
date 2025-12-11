using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class Lava : Area2D {
    [Export] private Water? _water;
    [Export] private AnimatedSprite2D _lavaSurfaceSprite = null!;
    [Export] private ColorRect _lavaRect = null!;
    
    private Fluid? _fluid;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _fluid ??= GetParent<Fluid>();
        
        // 防止起始点在水中导致开场死亡
        if (_water == null) {
            GD.PushError($"{this}: Water is not found.");
        } else if (_levelConfig != null && _fluid != null) {
            switch (_fluid.FluidType) {
                case Fluid.FluidTypeEnum.Lava:
                    Position = Position with { Y = _water.Position.Y };
                    break;
                case Fluid.FluidTypeEnum.Water:
                    Position = Position with { Y = _levelConfig.RoomHeight + 640 };
                    break;
            }
        }
    }
    public override void _PhysicsProcess(double delta) {
        // 计算水块位置
        string currentAnim = _lavaSurfaceSprite.Animation;
        int currentFrame = _lavaSurfaceSprite.Frame;
        
        Texture2D currentTexture = _lavaSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
        var screen = ScreenUtils.GetScreenRect(this);

        //Callable.From(() => {
        _lavaRect.GlobalPosition = new Vector2(
            screen.Position.X - 16f,
            Mathf.Max(
                GlobalPosition.Y + currentTexture.GetHeight() - 16f,
                screen.Position.Y - 16f
            )
        );
        //}).CallDeferred();
        
        if (_water == null) {
            GD.PushError($"{this}: Water is not found.");
        } else if (_levelConfig != null && _fluid != null) {
            switch (_fluid.FluidType) {
                case Fluid.FluidTypeEnum.Lava:
                    Position = Position with { Y = _water.Position.Y };
                    ResetPhysicsInterpolation();
                    break;
                case Fluid.FluidTypeEnum.Water:
                    Position = Position with { Y = _levelConfig.RoomHeight + 640 };
                    ResetPhysicsInterpolation();
                    break;
            }
        }
    }
}
