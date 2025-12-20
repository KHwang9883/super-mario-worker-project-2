using System.Collections.Generic;
using Godot;
using JetBrains.Annotations;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 老式移动平台（200-216）的加载器
/// </summary>
public partial class PlatformProcessor : ObjectProcessor {
    [Export] public GDC.Dictionary<PlatformType, PackedScene> PrefabByType { get; private set; } = [];
    [Export] public GDC.Dictionary<int, SmwpPlatformProcessorEntry> DataById { get; private set; } = [];
    
    public enum PlatformType {
        [UsedImplicitly] Falling,
        [UsedImplicitly] Horizontal,
        [UsedImplicitly] Vertical,
    }

    public override IEnumerable<Node>? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) {
        if (!DataById.TryGetValue(instance.Id, out var data)) {
            return base.CreateInstance(definition, instance);
        }
        return [CreatePlatform(data, data.Style, out _)];
    }

    /// <summary>
    /// 生成平台对象。
    /// 这个方法也会被 <see cref="NewPlatformProcessor"/> 使用。
    /// </summary>
    public Node CreatePlatform(
        SmwpPlatformProcessorEntry config,
        PlatformStyleSet.PlatformStyleEnum style,
        out Vector2 offset
    ) {
        // 创建平台节点
        var node = PrefabByType[config.Type].Instantiate<Node2D>();
        // 设置速度
        switch (node) {
            case PlatformHorizontal horizontal:
                horizontal.SpeedX = config.Speed;
                break;
            case PlatformVertical vertical:
                vertical.SpeedY = config.Speed;
                break;
        }
        // 设置样式
        if (node.TryGetComponent(out PlatformStyleSet? styleSet)) {
            styleSet.PlatformStyle = style;
            offset = styleSet.GetShapeFor(style).Size / 2;
        } else {
            offset = Vector2.Zero;
        }
        return node;
    }
}