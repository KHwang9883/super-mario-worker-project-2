using Godot;
using System;
using SMWP.Level;

public partial class RotoDiscMovement : Node {
    [Export] private PackedScene _rotoDiscCenterScene = null!;

    /* 这些数据要被读取器设置，所以需要公开 */
    [Export] public float Radius { get; set; } = 32f;
    [Export] public float Angle { get; set; }
    [Export] public float Speed { get; set; } = 1f;

    private Vector2 _originPosition;
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        _originPosition = _parent.Position;
        // 添加节点如果在 _Ready 中，必须延迟（Godot 魅力时刻）
        Callable.From(() => {
            var rotoDiscCenter = _rotoDiscCenterScene.Instantiate<Sprite2D>();
            rotoDiscCenter.Position = _originPosition;
            _parent.AddSibling(rotoDiscCenter);
        }).CallDeferred();

        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        levelConfig.SwitchSwitched += OnSwitchToggled;
        Angle += 90f;
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;

        if (Mathf.Abs(Speed) >= 25f) {
            _parent.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        }
        
        Angle = Mathf.Wrap(Angle + Speed, 0f, 360f);
        _parent.Position =
            new Vector2(
                _originPosition.X + Mathf.Sin(Mathf.DegToRad(Angle)) * Radius,
                _originPosition.Y + Mathf.Cos(Mathf.DegToRad(Angle)) * Radius
                );
    }

    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchTypeEnum) {
        if (switchTypeEnum != LevelConfig.SwitchTypeEnum.Magenta) return;
        //GD.Print($"Advanced {switchTypeEnum} Switch Switched!");
        Speed = -Speed;
    }
}
