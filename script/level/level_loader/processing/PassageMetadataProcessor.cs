using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 水管链接加载器
/// </summary>
public partial class PassageMetadataProcessor : ObjectProcessor {
    [Export] public required PackedScene PassageIn { get; set; }
    [Export] public required PackedScene PassageOut { get; set; }
    [Export] public GDC.Dictionary<PassageIn.PassageDirection, Vector2> EntranceOffsetByDirection { get; private set; } = [];
    [Export] public GDC.Dictionary<PassageIn.PassageDirection, Vector2> ExitOffsetByDirection { get; private set; } = [];

    private static volatile int _nextId;

    public override bool IsCreatedInstancePositioned() => true;

    public override IEnumerable<Node>? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (MetadataLengthIsInvalid("passage", metadata, 10)) {
            return base.CreateInstance(definition, instance);
        }
        if (!ClassicSmwpCodec.TryDecodeCoordinate(metadata.AsSpan()[..8], out var outPosition)) {
            GD.PushError($"Invalid passage out position: {instance.Metadata[..8]}");
            return base.CreateInstance(definition, instance);
        }
        char inDirectionCode = metadata[8];
        char outDirectionCode = metadata[9];
        if (!TryParseDirection(inDirectionCode, out var inDirection)) {
            GD.PushError($"Invalid passage in direction: {inDirectionCode}");
            return base.CreateInstance(definition, instance);
        }
        if (!TryParseDirection(outDirectionCode, out var outDirection)) {
            GD.PushError($"Invalid passage out direction: {outDirectionCode}");
            return base.CreateInstance(definition, instance);
        }
        return CreateInstance0(instance.Position, outPosition, inDirection, outDirection);
    }

    private static bool TryParseDirection(char serialized, out PassageIn.PassageDirection direction) {
        direction = (serialized - '0') switch {
            0 => global::PassageIn.PassageDirection.Right,
            1 => global::PassageIn.PassageDirection.Up,
            2 => global::PassageIn.PassageDirection.Left,
            3 => global::PassageIn.PassageDirection.Down,
            _ => (PassageIn.PassageDirection)(-1),
        };
        return direction >= 0;
    }

    private IEnumerable<Node> CreateInstance0(
        Vector2 inPosition,
        Vector2I outPosition,
        PassageIn.PassageDirection inDirection,
        PassageIn.PassageDirection outDirection
    ) {
        var id = Interlocked.Increment(ref _nextId);
        var inNode = PassageIn.Instantiate<Node2D>();
        var outNode = PassageOut.Instantiate<Node2D>();

        inNode.Position = inPosition + EntranceOffsetByDirection[inDirection];
        outNode.Position = outPosition + ExitOffsetByDirection[outDirection];
        
        if (inNode is PassageIn entrance) {
            entrance.PassageId = id;
            entrance.Direction = inDirection;
        }
        if (outNode is PassageOut exit) {
            exit.PassageId = id;
            exit.Direction = outDirection;
        }
        
        return [inNode, outNode];
    }
}