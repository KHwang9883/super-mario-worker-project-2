using Godot;
using System;
using SMWP.Level;

public partial class BrickCoinSwitch : Node {
    private LevelConfig? _levelConfig;
    private PackedScene _coinScene = GD.Load<PackedScene>("uid://b7p4i1t28bwi1");
    private Node2D _coinInstance = null!;
    private Node2D _parent = null!;
    private Vector2 _originPos;
    private Vector2 _idlePos;
    private bool _switch;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _levelConfig.SwitchSwitched += OnSwitchToggled;
        
        _parent = GetParent<Node2D>();
        _originPos = _parent.GlobalPosition;
        Callable.From(() => {
            _coinInstance = _coinScene.Instantiate<Node2D>();
            // 防止死循环
            _coinInstance.GetNode("CoinBrickSwitch").Free();
            _parent.AddSibling(_coinInstance);
            _idlePos = new Vector2(-3200f, _levelConfig.RoomHeight + 320f);
            _coinInstance.GlobalPosition = _idlePos;
        }).CallDeferred();
    }

    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchType) {
        if (switchType != LevelConfig.SwitchTypeEnum.Yellow) return;
        //GD.Print($"Advanced {switchTypeEnum} Switch Switched!");
        if (!IsInstanceValid(_parent) || !IsInstanceValid(_coinInstance)) return;
        _switch = !_switch;
        if (_switch) {
            _coinInstance.Position = _originPos;
            _parent.Position = _idlePos;
        } else {
            _coinInstance.Position = _idlePos;
            _parent.Position = _originPos;
        }
        _coinInstance.ResetPhysicsInterpolation();
        _parent.ResetPhysicsInterpolation();
    }

    public override void _ExitTree() {
        base._ExitTree();
        _coinInstance.QueueFree();
    }
}
