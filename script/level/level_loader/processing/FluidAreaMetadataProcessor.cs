using System;
using Godot;
using JetBrains.Annotations;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

public partial class FluidAreaMetadataProcessor : ObjectProcessor {
    public enum Type {
        [UsedImplicitly] Reusable,
        [UsedImplicitly] OneTime,
        [UsedImplicitly] Area,
    }
    
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (node is not FluidArea fluidArea) {
            return;
        }
        if (MetadataLengthIsInvalid("fluid area", metadata, 14)) {
            return;
        }
        // 目标高度
        if (ClassicSmwpCodec.TryDecodeCoordinateValue(metadata.AsSpan()[..4], out int targetHeight)) {
            GD.PushError($"Invalid target height {metadata[..4]} for fluid area");
            return;
        }
        // 流体速度
        if (!ClassicSmwpCodec.TryBase62Decode(metadata[4], out int speed)) {
            GD.PushError($"Invalid speed {metadata[4]} for fluid area");
            return;
        }
        // 类型
        if (!ClassicSmwpCodec.TryBase62Decode(metadata[5], out int type) || type < 0 || type > (int)Type.Area) {
            GD.PushError($"Invalid type {metadata[5]} for fluid area");
            return;
        }
        // 末端坐标
        if (!ClassicSmwpCodec.TryDecodeCoordinate(metadata.AsSpan()[6..], out var endPosition)) {
            GD.PushError($"Invalid end position {metadata[6..]} for fluid area");
        }

        // 给节点赋值
        fluidArea.TargetHeight = targetHeight;
        fluidArea.Speed = speed;
        if (type < (int)Type.Area) {
            // 单块型
            // 是否可复用
            fluidArea.Reusable = type == (int)Type.Reusable;
        } else {
            // 区域型
            fluidArea.Reusable = true;
            fluidArea.FluidRect = fluidArea.FluidRect with { Size = endPosition - instance.Position };
        }
    }
}