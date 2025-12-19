using Godot;
using System;
using SMWP.Level;

public partial class CoinBrickSwitch : Node {
    private LevelConfig? _levelConfig;
    private PackedScene _brickScene = GD.Load<PackedScene>("uid://b4b6pch7u4iem");
    private Node2D _brickInstance = null!;
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
            _brickInstance = _brickScene.Instantiate<Node2D>();
            // 防止死循环
            _brickInstance.GetNode("BrickCoinSwitch").Free();
            _parent.AddSibling(_brickInstance);
            _idlePos = new Vector2(-3200f, _levelConfig.RoomHeight + 320f);
            _brickInstance.GlobalPosition = _idlePos;
        }).CallDeferred();
    }

    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchType) {
        if (switchType != LevelConfig.SwitchTypeEnum.Yellow) return;
        //GD.Print($"Advanced {switchTypeEnum} Switch Switched!");
        if (!IsInstanceValid(_parent) || !IsInstanceValid(_brickInstance)) return;
        _switch = !_switch;
        if (_switch) {
            _brickInstance.Position = _originPos;
            _parent.Position = _idlePos;
        } else {
            _brickInstance.Position = _idlePos;
            _parent.Position = _originPos;
        }
        _brickInstance.ResetPhysicsInterpolation();
        _parent.ResetPhysicsInterpolation();
    }

    public override void _ExitTree() {
        base._ExitTree();
        _brickInstance.QueueFree();
    }
}
