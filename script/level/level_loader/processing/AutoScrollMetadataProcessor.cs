using Godot;
using SMWP.Level.Data;
using System;
using SMWP.Level.Loader.Processing;

public partial class AutoScrollMetadataProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        // 检查元数据长度
        if (MetadataLengthIsInvalid("auto scroll", metadata, 3)) {
            return;
        }
        // 自动卷轴速度
        if (!ClassicSmwpCodec.TryDecodeAutoScrollValue(metadata.AsSpan()[..3], 3, out int speed)) {
            GD.PushError($"Invalid speed {metadata[..3]} for auto scroll");
            return;
        }
        // 给自动卷轴属性赋值
        if (node is not AutoScroll autoScroll) return;
        autoScroll.Speed = speed;
    }
}
