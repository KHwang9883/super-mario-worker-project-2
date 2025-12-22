using System;
using System.Collections.Generic;
using Godot;
using JetBrains.Annotations;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

public partial class FluidAreaMetadataProcessor : ObjectProcessor {
    [Export] public required PackedScene FluidBlockPrefab { get; set; }
    [Export] public required PackedScene FluidAreaPrefab { get; set; }
    
    public enum Type {
        [UsedImplicitly] Reusable,
        [UsedImplicitly] OneTime,
        [UsedImplicitly] Area,
    }

    public override bool IsCreatedInstancePositioned() => true;

    public override IEnumerable<Node>? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (MetadataLengthIsInvalid("fluid control", metadata, 14)) {
            return [];
        }
        // 类型
        if (!TryGetType(metadata, out var type)) {
            return [];
        }
        var node = (type == Type.Area ? FluidAreaPrefab : FluidBlockPrefab).Instantiate();
        if (node is Node2D control) {
            control.Position = instance.Position + (type == Type.Area ? Vector2.Zero : new Vector2(16, 16));
        }
        return [node];
    }

    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (MetadataLengthIsInvalid("fluid control", metadata, 14)) {
            return;
        }
        // 目标高度
        if (!ClassicSmwpCodec.TryDecodeCoordinateValue(metadata.AsSpan()[..4], out int targetHeight)) {
            GD.PushError($"Invalid target height {metadata[..4]} for fluid area");
            return;
        }
        // 流体速度
        if (!ClassicSmwpCodec.TryBase62Decode(metadata[4], out int speed)) {
            GD.PushError($"Invalid speed {metadata[4]} for fluid area");
            return;
        }
        // 类型
        if (!TryGetType(metadata, out var type)) {
            return;
        }
        // 末端坐标
        if (!ClassicSmwpCodec.TryDecodeCoordinate(metadata.AsSpan()[6..], out var endPosition)) {
            GD.PushError($"Invalid end position {metadata[6..]} for fluid area");
        }

        // 给节点赋值
        if (node is FluidArea fluidArea) {
            fluidArea.TargetHeight = targetHeight;
            fluidArea.Speed = speed;
            fluidArea.FluidRect = fluidArea.FluidRect with { Size = endPosition - instance.Position };
        } else if (node.TryGetComponent(out FluidBlock? fluidBlock)) {
            fluidBlock.TargetHeight = targetHeight;
            fluidBlock.Speed = speed;
            fluidBlock.BumpableOneShot = type == Type.OneTime;
        }
    }

    private static bool TryGetType(string metadata, out Type type) {
        if (!ClassicSmwpCodec.TryBase62Decode(metadata[5], out int typeId) || typeId < 0 || typeId > (int)Type.Area) {
            GD.PushError($"Invalid type {metadata[5]} for fluid area");
            type = default;
            return false;
        }
        type = (Type)typeId;
        return true;
    }
}