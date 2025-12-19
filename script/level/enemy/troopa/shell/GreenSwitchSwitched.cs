using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Enemy;
using SMWP.Level.Physics;

public partial class GreenSwitchSwitched : Node {
    private bool _shellSet;
    private bool _shellDir;
    
    private LevelConfig? _levelConfig;
    
    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _levelConfig.SwitchSwitched += OnSwitchToggled;
        
        // 组件 Ready 的时候父节点还没 Ready，因此延迟调用
        Callable.From(() => {
            var parent = GetParent();
            if (parent.HasMeta("ShellSwitchDirection")) {
                _shellDir = parent.GetMeta("ShellSwitchDirection").AsBool();
                _shellSet = true;
                //GD.Print($"GreenSwitchSwitched: _shellSet from meta: {_shellSet}");
                //GD.Print($"GreenSwitchSwitched: _shellDir from meta: {_shellDir}");
            }
        }).CallDeferred();
    }
    
    public void OnSwitchToggled(LevelConfig.SwitchTypeEnum switchType) {
        if (switchType != LevelConfig.SwitchTypeEnum.Green) return;
        
        var parent = GetParent();
        if (!parent.HasMeta("ShellSwitchDirection")) {
            var basicMovementComponent = parent.GetNodeOrNull<BasicMovement>("BasicMovement");
            if (basicMovementComponent != null) {
                _shellDir = Math.Sign(basicMovementComponent.SpeedX) > 0;
                _shellSet = true;
            }
        }
        
        var dieComponent = GetParent()?.GetNode<EnemyDie>("Die");
        //GD.Print($"GreenSwitchSwitched: _shellSet setting meta: {_shellSet}");
        //GD.Print($"GreenSwitchSwitched: _shellDir setting meta: {_shellDir}");
        Callable.From(() => {
            dieComponent?.OnDied(EnemyDie.EnemyDieEnum.CreateInstance, _shellSet, _shellDir);
        }).CallDeferred();
    }

    public override void _ExitTree() {
        base._ExitTree();
        if (_levelConfig == null) return;
        _levelConfig.SwitchSwitched -= OnSwitchToggled;
    }
}
