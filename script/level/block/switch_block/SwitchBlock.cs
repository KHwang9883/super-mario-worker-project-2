using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SMWP;
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
        if (GameManager.IsColorAccessibilityMode) _ani?.Play("color-vision");
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        // 记录非被顶状态开关砖当前动画进度，保持所有开关砖同步
        if (_ani == null) return;
        if (GameManager.IsColorAccessibilityMode) return;
        if (_ani.Animation.Equals("hit")) return;
        _frameProgress = _ani.FrameProgress;
        _frame = _ani.Frame;
    }
    
    protected override void OnBlockBump() {
        base.OnBlockBump();
        if (!GameManager.IsColorAccessibilityMode) _ani?.Play("hit");
        
        // Switch Toggle
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null");
            return;
        }

        // 无第二功能
        if (!_levelConfig.AdvancedSwitch) {
            _levelConfig.ToggleSwitch(SwitchType, !_levelConfig.Switches.GetValueOrDefault(SwitchType, false));
        }
        
        // 开关砖第二功能
        else {
            // 黑色或白色以外的开关砖正常切
            if (SwitchType != LevelConfig.SwitchTypeEnum.White && SwitchType != LevelConfig.SwitchTypeEnum.Kohl) {
                _levelConfig.ToggleSwitch(SwitchType, !_levelConfig.Switches.GetValueOrDefault(SwitchType, false));
                return;
            }
            
            // 白色开关砖第二功能：切换除了黑色的所有颜色
            if (SwitchType == LevelConfig.SwitchTypeEnum.White) {
                var nonKohlSwitchTypes = Enum.GetValues<LevelConfig.SwitchTypeEnum>()
                    .Where(type => type != LevelConfig.SwitchTypeEnum.Kohl)
                    .ToList();
            
                // 遍历并切换每个非黑色开关的状态，并不传递第二功能
                foreach (var switchType in nonKohlSwitchTypes) {
                    var currentState = _levelConfig.Switches.GetValueOrDefault(switchType, false);
                    _levelConfig.ToggleSwitch(switchType, !currentState, true);
                }
            }
            
            // 黑色开关砖第二功能：切换除了自身和白色的所有颜色的第二功能
            if (SwitchType == LevelConfig.SwitchTypeEnum.Kohl) {
                var nonWhiteSwitchTypes = Enum.GetValues<LevelConfig.SwitchTypeEnum>()
                    .Where(type => type != LevelConfig.SwitchTypeEnum.White
                                   && type != LevelConfig.SwitchTypeEnum.Kohl)
                    .ToList();
            
                // ↓ 但是黑色的要手动切一下
                _levelConfig.ToggleSwitch(LevelConfig.SwitchTypeEnum.Kohl, !_levelConfig.Switches.GetValueOrDefault(SwitchType, false));
                    
                // 遍历并切换每个非白色非黑色开关的状态，传递第二功能且不切换非黑开关砖状态
                foreach (var switchType in nonWhiteSwitchTypes) {
                    var currentState = _levelConfig.Switches.GetValueOrDefault(switchType, false);
                    _levelConfig.ToggleSwitch(switchType, !currentState, false, true);
                }
            }
        }
    }
    protected override void OnBumped() {
        base.OnBumped();
        if (_ani == null) return;
        if (GameManager.IsColorAccessibilityMode) return;
        _ani.Play("default");
        _ani.Frame = _frame;
        _ani.FrameProgress = _frameProgress;
    }
}
