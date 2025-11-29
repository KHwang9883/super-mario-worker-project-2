using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class Water : Area2D {
    [Export] private AnimatedSprite2D _waterSurfaceSprite = null!;
    [Export] private ColorRect _waterRect = null!;
    private LevelConfig? _levelConfig;
    public enum FluidTypeEnum { Water, Lava }
    private FluidTypeEnum _fluidType = FluidTypeEnum.Water;
    private bool _autoFluid;
    private float _t1;
    private float _t2;
    private float _speed;
    private int _delay;
    private int _delayTimer;
    private float _target;
    private bool _t1OrT2;

    // 测试水块跟随用，可以删除
    //private double _testWaterPhase = 233f;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        
        Position = Position with { Y = _levelConfig.WaterHeight };
        _fluidType = _levelConfig.FluidType;
        _autoFluid = _levelConfig.AutoFluid;
        if (!_autoFluid) return;
        _t1 = _levelConfig.FluidT1;
        _t2 = _levelConfig.FluidT2;
        _speed = _levelConfig.FluidSpeed;
        _delay = _levelConfig.FluidDelay;
        _target = _t1;
    }
    public override void _PhysicsProcess(double delta) {
        
        string currentAnim = _waterSurfaceSprite.Animation;
        int currentFrame = _waterSurfaceSprite.Frame;
        
        Texture2D currentTexture = _waterSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
        // 流体运动
        if (_autoFluid) {
            if (_delayTimer < _delay * 16) {
                _delayTimer++;
            } else {
                Position = Position with { Y = Position.Y + Mathf.Sign(_target - Position.Y) * (_speed * 0.4f) };
                if (Mathf.Abs(Position.Y - _target) < (_speed * 0.4f)) {
                    Position = Position with { Y = _target };
                    _t1OrT2 = !_t1OrT2;
                    _target = (_t1OrT2) ? ((_t2 > -64) ? _t2 : _t1) : _t1;
                    _delayTimer = 0;
                }
            }
        }
        
        // 计算水块位置
        var screen = ScreenUtils.GetScreenRect(this);

        //Callable.From(() => {
            _waterRect.GlobalPosition = new Vector2(
                screen.Position.X - 16f,
                Mathf.Max(
                    GlobalPosition.Y + currentTexture.GetHeight() - 1f,
                    screen.Position.Y - 16f
                )
            );
        //}).CallDeferred();

        // 测试水块跟随用，可以删除
        //Position = new Vector2(Position.X, 0f + (float)Math.Sin(_testWaterPhase) * 128);
        //_testWaterPhase += delta * 2f;
    }
    public void SetWaterHeight(float height) {
        Position = Position with { Y = height };
        _autoFluid = false;
    }
}