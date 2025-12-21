using Godot;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 探照灯额外数据处理器
/// </summary>
public partial class RotoDiscMetadataProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        // 检查元数据长度
        if (MetadataLengthIsInvalid("roto-disc", metadata, 9)) {
            return;
        }
        // 给探照灯属性赋值
        if (!node.TryGetComponent(out RotoDiscMovement? movement)) {
            return;
        }
        if (float.TryParse(metadata[..3], out var radius)) {
            movement.Radius = radius;
        }
        if (float.TryParse(metadata[3..6], out var angle)) {
            movement.Angle = angle;
        }
        if (float.TryParse(metadata[6..], out var speed)) {
            movement.Speed = speed;
        }
    }
}