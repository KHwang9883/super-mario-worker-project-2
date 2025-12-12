using Godot;
using System;
using System.Collections.Generic;
using SMWP.Level;
using SMWP.Level.Block;

public partial class SwitchBlock : BlockHit {
    [Export] public LevelConfig.SwitchTypeEnum SwitchType;
    
    [Export] public Godot.Collections.Dictionary<LevelConfig.SwitchTypeEnum, SpriteFrames> Sprites = null!;
    
    private AnimatedSprite2D? _ani;
    private static float _frameProgress;
    private static int _frame;

    private LevelConfig? _levelConfig;
    
    public override void _Ready() {
        base._Ready();
        if (Sprite is AnimatedSprite2D ani) {
            _ani = ani;
        }
        
        // For test only
        //SwitchType = (LevelConfig.SwitchTypeEnum)Enum.GetValues(typeof(LevelConfig.SwitchTypeEnum)).GetValue(new Random().Next(Enum.GetValues(typeof(LevelConfig.SwitchTypeEnum)).Length))!;
        
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _ani?.SetSpriteFrames(Sprites[SwitchType]);
        if (LevelManager.IsColorAccessibilityMode) _ani?.Play("color-vision");
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        // 记录非被顶状态开关砖当前动画进度，保持所有开关砖同步
        if (_ani == null) return;
        if (LevelManager.IsColorAccessibilityMode) return;
        if (_ani.Animation.Equals("hit")) return;
        _frameProgress = _ani.FrameProgress;
        _frame = _ani.Frame;
    }
    
    protected override void OnBlockBump() {
        base.OnBlockBump();
        if (!LevelManager.IsColorAccessibilityMode) _ani?.Play("hit");
        
        // Switch Toggle
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null");
            return;
        }
        _levelConfig.ToggleSwitch(SwitchType, !_levelConfig.Switches.GetValueOrDefault(SwitchType, false));
    }
    protected override void OnBumped() {
        base.OnBumped();
        if (_ani == null) return;
        if (LevelManager.IsColorAccessibilityMode) return;
        _ani.Play("default");
        _ani.Frame = _frame;
        _ani.FrameProgress = _frameProgress;
    }
}
