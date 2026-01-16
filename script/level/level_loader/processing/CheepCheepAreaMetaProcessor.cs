using System;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 飞 / 游鱼区域元数据读取处理器
/// </summary>
public partial class CheepCheepAreaMetaProcessor : ObjectProcessor {
    [Export] public CheepCheepArea.CheepAreaTypeEnum Type { get; set; }

    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (node is not CheepCheepArea area) {
            return;
        }
        // 检查元数据长度
        if (MetadataLengthIsInvalid("cheep cheep", metadata, 9)) {
            return;
        }
        // 读取末端坐标
        if (!ClassicSmwpCodec.TryDecodeCoordinate(metadata.AsSpan()[..8], out var end)) {
            GD.PushError($"Unknown cheep cheep end coordinate. metadata string is {metadata}");
            return;
        }
        // 读取并检查鱼类型
        int cheepType = metadata[8] - '0';
        if (cheepType is < 0 or > (int)CheepCheepArea.CheepTypeEnum.Spike) {
            GD.PushError($"Unknown cheep type {cheepType}. metadata string is {metadata}");
            return;
        }
        // 赋值
        area.CheepAreaDirection =
            (end.X > instance.Position.X)
                ? CheepCheepArea.CheepAreaDirectionEnum.Left
                : CheepCheepArea.CheepAreaDirectionEnum.Right;
        area.CheepAreaType = Type;
        area.CheepAreaRect =
            new Rect2(instance.Position, end - instance.Position + new Vector2(32f, 32f));
        switch (Type) {
            case CheepCheepArea.CheepAreaTypeEnum.Swim:
                area.CheepType = cheepType switch {
                    0 => CheepCheepArea.CheepTypeEnum.Red,
                    1 => CheepCheepArea.CheepTypeEnum.Red,
                    2 => CheepCheepArea.CheepTypeEnum.Green,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                area.CheepAreaLevel = cheepType switch {
                    0 => CheepCheepArea.CheepAreaLevelEnum.Level1,
                    1 => CheepCheepArea.CheepAreaLevelEnum.Level2,
                    2 => CheepCheepArea.CheepAreaLevelEnum.Level1,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                break;
            case CheepCheepArea.CheepAreaTypeEnum.Fly:
                area.CheepType = cheepType switch {
                    0 => CheepCheepArea.CheepTypeEnum.Red,
                    1 => CheepCheepArea.CheepTypeEnum.Red,
                    2 => CheepCheepArea.CheepTypeEnum.Blue,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                area.CheepAreaLevel = cheepType switch {
                    0 => CheepCheepArea.CheepAreaLevelEnum.Level1,
                    1 => CheepCheepArea.CheepAreaLevelEnum.Level2,
                    2 => CheepCheepArea.CheepAreaLevelEnum.Level1,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                break;
        }
    }
}