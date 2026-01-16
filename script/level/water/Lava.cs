using Godot;
using System;
using SMWP.Level;
using SMWP.Util;

public partial class Lava : Area2D {
    [Export] private Water? _water;
    [Export] private AnimatedSprite2D _lavaSurfaceSprite = null!;
    [Export] private Sprite2D _lavaRect = null!;

    [Export] private float _waterPosYOffset = -10f;
    
    private Fluid? _fluid;
    private LevelConfig? _levelConfig;
    
    private bool _disappear;
    private uint _originCollisionLayer;

    private bool _meLoveTallCastleSoMuch;
    
    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _fluid ??= GetParent<Fluid>();
        
        _levelConfig.SwitchSwitched += OnSwitchToggled;
        _originCollisionLayer = CollisionLayer;
        
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
        TallCastleCheck();
        
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
            if (_disappear) return;
            switch (_fluid.FluidType) {
                case Fluid.FluidTypeEnum.Lava:
                    Position = Position with { Y = _water.Position.Y - _waterPosYOffset};
                    ResetPhysicsInterpolation();
                    break;
                case Fluid.FluidTypeEnum.Water:
                    Position = Position with { Y = _levelConfig.RoomHeight + 640 };
                    ResetPhysicsInterpolation();
                    break;
            }
        }
    }
    
    // 蓝色开关砖第二功能
    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchType) {
        // 流体消失或再现
        if (switchType != LevelConfig.SwitchTypeEnum.Blue) return;
        _disappear = !_disappear;
        if (_disappear) {
            Visible = false;
            CollisionLayer = 0;
        } else {
            Visible = true;
            CollisionLayer = _originCollisionLayer;
        }
    }
    
    // SMWP1 关卡默认水位大于等于 1000000 像素的情况
    // 对应关卡：Tall Castle 5(Beta).smwp
    public void TallCastleCheck() {
        if (_water == null) {
            return;
        }
        if (_water.ReallyLoveTallCastle == Water.MeLoveTallCastle.Collapse && !_meLoveTallCastleSoMuch) {
            _disappear = true;
            CollisionLayer = 0;
            _meLoveTallCastleSoMuch = true;
            //GD.Print("Tall Lava");
        }
    }
}
