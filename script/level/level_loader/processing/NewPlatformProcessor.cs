using System.Collections.Generic;
using Godot;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 运输桥（新）（225）的加载器
/// </summary>
public partial class NewPlatformProcessor : ObjectProcessor {
    [Export] public required PlatformProcessor DataSource { get; set; }
    [Export] public required SmwpPlatformProcessorEntry[] ConfigByType { get; set; } = [];

    public override bool IsCreatedInstancePositioned() => true;

    public override IEnumerable<Node>? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (MetadataLengthIsInvalid("new platform", metadata, 6)) {
            return base.CreateInstance(definition, instance);
        }
        if (!int.TryParse(metadata[..3], out var type) || !int.TryParse(metadata[3..], out var style)) {
            GD.PushError($"Invalid metadata \"{metadata}\" for new platform");
            return base.CreateInstance(definition, instance);
        }
        if (type < 0 || type >= ConfigByType.Length) {
            GD.PushError($"Invalid platform type {type} for new platform");
            return base.CreateInstance(definition, instance);
        }
        return CreateInstance0(ConfigByType[type], (PlatformStyleSet.PlatformStyleEnum)style, instance.Position);
    }

    private IEnumerable<Node> CreateInstance0(
        SmwpPlatformProcessorEntry config,
        PlatformStyleSet.PlatformStyleEnum style,
        Vector2 position
    ) {
        var node = DataSource.CreatePlatform(config, style, out var offset);
        if (node is Node2D platform) {
            platform.Position = position + offset;
        }
        return [node];
    }
}