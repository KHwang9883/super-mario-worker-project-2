using Godot;
using System;
using System.Collections.Generic;
using SMWP.Level;

public partial class DottedLineBlock : StaticBody2D {
    [Export] private AnimatedSprite2D _ani = null!;
    
    [Export] public LevelConfig.SwitchTypeEnum SwitchType;
    [Export] public bool Solid = true;
    public bool JustSolid;

    [Export] private Area2D _celesteDetect = null!;

    [Export] private SmwpPointLight2D _smwpPointLight2D = null!;
    
    [Export] public Godot.Collections.Dictionary<LevelConfig.SwitchTypeEnum, SpriteFrames> Sprites = null!;
    
    private bool _originStatus;
    private bool _initialCheck;
    
    // 定义碰撞层掩码常量（用位移明确表示二进制位）
    // 1 << 10 等价于 2^10 = 1024（第 11 位为 1）
    private const uint CollisionLayerDetect = 1 << 10;
    // 1 << 0 等价于 2^0 = 1（第 1 位为 1）
    private const uint CollisionLayerSolid = 1 << 0;
    // 组合掩码：1024 + 1 = 1025
    private const uint CollisionLayerSolidFull = CollisionLayerDetect | CollisionLayerSolid;

    private LevelConfig? _levelConfig;
    
    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _initialCheck = _levelConfig.Switches.GetValueOrDefault(SwitchType, false) ^ Solid;
        _originStatus = Solid;
        _ani.SetSpriteFrames(Sprites[SwitchType]);
        
        if (!LevelManager.IsColorAccessibilityMode) return;
        _ani.Play("color-vision");
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null");
            return;
        }

        // 用于玩家对于开关砖变实心的专门判断
        JustSolid = false;
        
        // 切换虚实状态
        var newCheck = _levelConfig.Switches.GetValueOrDefault(SwitchType, false) ^ _originStatus;
        if (_initialCheck != newCheck) {
            
            // 蔚蓝模式
            if (_levelConfig.CelesteStyleSwitch
                && _celesteDetect.GetOverlappingBodies().Count > 0 && !Solid) return;
            
            _initialCheck = newCheck;
            Solid = !Solid;
            JustSolid = Solid;
        }
        
        CollisionLayer = Solid ? CollisionLayerSolidFull : CollisionLayerDetect;
        Modulate = Modulate with { A = Solid ? 1f : 0.441f };
    }

    public override void _Process(double delta) {
        _smwpPointLight2D.LightRadius = !Solid ? 0f : 1f;
    }
}
