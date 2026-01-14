using Godot;
using System;
using System.Linq.Expressions;
using Godot.Collections;
using SMWP.Level;
using SMWP.Util;

public partial class SmwpPointLight2D : Node2D {
    [Export] private Marker2D _marker = null!;
    [Export] public bool Enabled;
    [Export] public Array<int>? Smwp1LightId;
    [Export] public bool IsLavaLight { get; private set; }
    
    public bool Activate;
    public Vector2 LightPosition;
    public float LightRadius = 1f;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        
        Callable.From(() => {
            // Parse Smwp1LightObjectString
            var lightObjectStr = _levelConfig.Smwp1LightObjectString;
            if (Smwp1LightId == null) {
                return;
            }
            foreach (var id in Smwp1LightId) {
                var trueId = id - 1;
                // 越你爷爷
                // ——梗来自 SMBX 圈
                if (trueId < 0 || trueId > lightObjectStr.Length - 1) continue;
                if (lightObjectStr[trueId] == '1') {
                    Enabled = true;
                    //GD.Print($"Light of {GetParent().Name}: Enabled is {Enabled}.");
                }
            }
        }).CallDeferred();
    }

    // 发光默认一直禁用，直到入屏启用
    public override void _PhysicsProcess(double delta) {
        if (!Enabled) return;
        Activate = true;
        LightPosition = _marker.GlobalPosition;
        
        // 全局流体岩浆的特殊情形
        if (IsLavaLight) {
            GlobalPosition = GlobalPosition with { X = ScreenUtils.GetScreenRect(this).Position.X };
        }
    }

    public void OnScreenExited() {
        if (!Enabled) return;
        Activate = false;
    }
}
