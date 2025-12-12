using Godot;
using System;
using SMWP.Level;

public partial class RotoDiscMovement : Node {
    [Export] private PackedScene _rotoDiscCenterScene = null!;

    [Export] private float _radius = 32f;
    [Export] private float _angle;
    [Export] private float _speed = 1f;

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
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;
        _angle = Mathf.Wrap(_angle + _speed, 0f, 360f);
        _parent.Position =
            new Vector2(
                _originPosition.X + Mathf.Sin(Mathf.DegToRad(_angle)) * _radius,
                _originPosition.Y + Mathf.Cos(Mathf.DegToRad(_angle)) * _radius
                );
    }

    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchTypeEnum) {
        if (switchTypeEnum != LevelConfig.SwitchTypeEnum.Magenta) return;
        //GD.Print($"Advanced {switchTypeEnum} Switch Switched!");
        _speed = -_speed;
    }
}
