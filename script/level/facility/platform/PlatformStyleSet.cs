using Godot;
using System;
using Godot.Collections;
using JetBrains.Annotations;

public partial class PlatformStyleSet : Node {
    [Export] private AnimatedSprite2D _animatedSprite = null!;
    [Export] private CollisionShape2D _collisionShape2D = null!;
    
    [Export] private RectangleShape2D _bridgeLong = GD.Load<RectangleShape2D>("uid://cjykfgkno6iho");
    [Export] private RectangleShape2D _bridgeShort = GD.Load<RectangleShape2D>("uid://c0l4yfx36brqr");
    [Export] private RectangleShape2D _cloud = GD.Load<RectangleShape2D>("uid://b434y6vlpp2m8");
    [Export] private RectangleShape2D _castleLong = GD.Load<RectangleShape2D>("uid://getamfhougth");
    [Export] private RectangleShape2D _castleNormal = GD.Load<RectangleShape2D>("uid://cpjxloxiaskl5");
    
    [Export] private Dictionary<PlatformStyleEnum, Shape2D> _shapeMapping = null!;
    
    public enum PlatformStyleEnum {
        [UsedImplicitly] RedLong,
        [UsedImplicitly] RedShort,
        [UsedImplicitly] Cloud,
        [UsedImplicitly] YellowLong,
        [UsedImplicitly] YellowShort,
        [UsedImplicitly] BlueLong,
        [UsedImplicitly] BlueShort,
        [UsedImplicitly] GreenLong,
        [UsedImplicitly] GreenShort,
        [UsedImplicitly] WhiteLong,
        [UsedImplicitly] WhiteShort,
        [UsedImplicitly] GreyLong,
        [UsedImplicitly] GreyShort,
        [UsedImplicitly] PurpleLong,
        [UsedImplicitly] PurpleShort,
        [UsedImplicitly] CastleLong,
        [UsedImplicitly] CastleNormal,
        [UsedImplicitly] CastleOrangeLong,
        [UsedImplicitly] CastleOrangeNormal,
    }
    [Export] public PlatformStyleEnum PlatformStyle;
    
    [Export] private CollisionShape2D? _overlapCollisionShape2D;

    public override void _Ready() {
        _animatedSprite.Animation = PlatformStyle.ToString();

        if (!_shapeMapping.TryGetValue(PlatformStyle, out var shape)) return;
        _collisionShape2D.Shape = shape;
        
        // 碰撞箱设置，目前仅水平移动平台用于转向标记的重叠检测
        if (_overlapCollisionShape2D == null) return;
        _overlapCollisionShape2D.Shape = shape;
    }

    /// <summary>
    /// 获取平台的形状，用于在加载器加载平台时动态调整 Offset。
    /// </summary>
    public RectangleShape2D GetShapeFor(PlatformStyleEnum style) {
        return _shapeMapping.TryGetValue(style, out var shape) && shape is RectangleShape2D rectangle 
            ? rectangle
            : _bridgeLong;
    }
}
