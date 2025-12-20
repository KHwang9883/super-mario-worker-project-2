using System.Collections.Generic;
using Godot;
using JetBrains.Annotations;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

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
        // 生成平台对象
        var node = PrefabByType[data.Type].Instantiate<Node2D>();
        // 设置速度
        switch (node) {
            case PlatformHorizontal horizontal:
                horizontal.SpeedX = data.Speed;
                break;
            case PlatformVertical vertical:
                vertical.SpeedY = data.Speed;
                break;
        }
        // 设置样式
        if (node.TryGetComponent(out PlatformStyleSet? style)) {
            style.PlatformStyle = data.Style;
        }
        return [node];
    }
}