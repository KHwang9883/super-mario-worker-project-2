using Godot;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 开关砖（开关）和阴阳砖的元数据加载器
/// 适用于 227, 228. 229
/// </summary>
public partial class SwitchBlockMetaProcessor : ObjectProcessor {
    [Export] public GDC.Dictionary<LevelConfig.SwitchTypeEnum, SpriteFrames> SpriteByType { get; private set; } = [];
    [Export] public GDC.Dictionary<int, bool> IsSolidById { get; private set; } = [];

    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (MetadataLengthIsInvalid("switch block", metadata, 1)) {
            return;
        }
        if (!TryParseSwitchType(metadata[0], out var type)) {
            GD.PushError($"Unknown switch type: {type}");
            return;
        }
        if (node is DottedLineBlock block) {
            // 设置阴阳砖样式
            block.SwitchType = type;
            // 设置阴阳砖开关
            if (IsSolidById.TryGetValue(instance.Id, out var isSolid)) {
                block.Solid = isSolid;
            }
        } else if (node.TryGetComponent(out AnimatedSprite2D? sprite)) {
            // 设置开关砖样式
            sprite.SpriteFrames = SpriteByType[type];
        }
    }

    private static bool TryParseSwitchType(char metadata, out LevelConfig.SwitchTypeEnum result) {
        if (ClassicSmwpCodec.TryBase62Decode(metadata, out int decoded)) {
            result = (LevelConfig.SwitchTypeEnum)decoded;
            return true;
        } else {
            result = default;
            return false;
        }
    }
}