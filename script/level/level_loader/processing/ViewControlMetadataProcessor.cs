using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;
using SMWP.Util;

public partial class ViewControlMetadataProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        // 检查元数据长度
        if (MetadataLengthIsInvalid("view control", metadata, 8)) {
            return;
        }
        // 镜头范围
        if (!ClassicSmwpCodec.TryDecodeCoordinateValue(metadata.AsSpan()[..4], out int viewWidth)) {
            GD.PushError($"Invalid view width {metadata[..4]} for view control");
            return;
        }
        if (!ClassicSmwpCodec.TryDecodeCoordinateValue(metadata.AsSpan()[4..8], out int viewHeight)) {
            GD.PushError($"Invalid view height {metadata[4..8]} for view control");
            return;
        }
        // 给镜头属性赋值
        if (node is not ViewControl viewControl) return;
        viewControl.ViewRect = new Rect2(instance.Position, new Vector2(viewWidth, viewHeight));
    }
}
