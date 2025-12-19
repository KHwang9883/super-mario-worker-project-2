using Godot;
using System;
using SMWP.Level;

public partial class PlatformVertical : AnimatableBody2D, ISteppable {
    [Signal]
    public delegate void SteppedEventHandler();
    
    [Export] public float SpeedY = 1f;
    private float _textureHeight;
    private float _topLimit = -32f;
    private float _bottomLimit = 0f;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _levelConfig.SwitchSwitched += OnSwitchToggled;
        _bottomLimit += _levelConfig.RoomHeight;
        var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _textureHeight = animatedSprite2D.SpriteFrames.GetFrameTexture(
            animatedSprite2D.Animation, animatedSprite2D.Frame
            ).GetSize().Y;
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
        } else {
            Position += new Vector2(0f, SpeedY);
            
            // 由于 GM8 版本中中心点在左上角，因此为 offset 处理
            var truePositionY = Position.Y - _textureHeight / 2f;
            if (truePositionY < _topLimit) {
                Position = Position with { Y = _bottomLimit + _textureHeight / 2f };
                ResetPhysicsInterpolation();
            }
            if (truePositionY > _bottomLimit) {
                Position = Position with { Y = _topLimit + _textureHeight / 2f };
                ResetPhysicsInterpolation();
            }
        }
    }
    public void OnStepped() {
        EmitSignal(SignalName.Stepped);
    }
    
    // 开关砖第二功能
    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchTypeEnum) {
        if (switchTypeEnum != LevelConfig.SwitchTypeEnum.Cyan) return;
        //GD.Print($"Advanced {switchTypeEnum} Switch Switched!");
        SpeedY = -SpeedY;
    }
}
