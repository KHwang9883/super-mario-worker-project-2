using Godot;
using System;
using SMWP.Level;
using SMWP.Util;

public partial class Water : Area2D {
    [Signal]
    public delegate void PlaySoundWaterLevelEventHandler();
    [Signal]
    public delegate void PlaySoundLavaLevelEventHandler();
    
    [Export] private AnimatedSprite2D _waterSurfaceSprite = null!;
    [Export] private Sprite2D _waterRect = null!;
    [Export] private CollisionShape2D _collisionShape2D { get; set; } = null!;
    
    private Fluid? _fluid;
    private LevelConfig? _levelConfig;
    private bool _autoFluid;
    private float _t1;
    private float _t2;
    private float _speed;
    private int _delay;
    private int _delayTimer;
    private float _target;
    private bool _t1OrT2;

    private bool _fluidControlSet;
    private float _fluidControlTargetHeight;
    private float _fluidControlSpeed;

    private bool _disappear;
    private uint _originCollisionLayer;
    
    private Vector2 _collisionShapeOriginPos;

    public override void _Ready() {
        _fluid ??= GetParent<Fluid>();
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);

        _levelConfig.SwitchSwitched += OnSwitchToggled;
        _originCollisionLayer = CollisionLayer;

        _collisionShapeOriginPos = _collisionShape2D.Position;
        
        // Position 初始化见 LevelConfig
        _autoFluid = _levelConfig.AutoFluid;
        if (!_autoFluid) return;
        _t1 = _levelConfig.FluidT1;
        _t2 = _levelConfig.FluidT2;
        _speed = _levelConfig.FluidSpeed;
        _delay = _levelConfig.FluidDelay;
        _target = _t1;
    }
    public override void _PhysicsProcess(double delta) {
        // 流体控件设置目标高度
        if (_fluidControlSet) {
            Position = Position with {
                Y = Mathf.MoveToward(Position.Y, _fluidControlTargetHeight, _fluidControlSpeed),
            };
            if (Math.Abs(Position.Y - _fluidControlTargetHeight) < _fluidControlSpeed) {
                Position = Position with { Y = _fluidControlTargetHeight };
                _fluidControlSet = false;
            }
        }
        
        // 自动流体运动
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
        string currentAnim = _waterSurfaceSprite.Animation;
        int currentFrame = _waterSurfaceSprite.Frame;
        
        Texture2D currentTexture = _waterSurfaceSprite.GetSpriteFrames().GetFrameTexture(currentAnim, currentFrame);
        
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
        
        // 流体情况
        if (_fluid == null) return;
        if (_disappear) return;
        switch (_fluid.FluidType) {
            case Fluid.FluidTypeEnum.Water:
                Visible = true;
                _collisionShape2D.Position = _collisionShapeOriginPos;
                break;
            case Fluid.FluidTypeEnum.Lava:
                Visible = false;
                _collisionShape2D.Position = new Vector2(0f, 16f);
                break;
        }
    }
    
    // 场景控制元件直接设置水位
    public void SetWaterHeight(float height) {
        Position = Position with { Y = height };
        _autoFluid = false;
        _fluidControlTargetHeight = height;
    }
    
    // 流体控制元件设置水位
    public void FluidControlSet(float targetHeight, float speed) {
        _autoFluid = false;
        
        _fluidControlSet = true;
        _fluidControlTargetHeight = targetHeight;
        _fluidControlSpeed = speed;
        
        if (_levelConfig == null) return;
        switch (_levelConfig.FluidType) {
            case Fluid.FluidTypeEnum.Water:
                EmitSignal(SignalName.PlaySoundWaterLevel);
                break;
            case Fluid.FluidTypeEnum.Lava:
                CollisionLayer = 0;
                EmitSignal(SignalName.PlaySoundLavaLevel);
                break;
        }
    }

    public float GetTargetHeight() {
        return _fluidControlTargetHeight;
    }
    
    // 红色开关砖第二功能见 LevelConfig
    
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
}