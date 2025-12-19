using System;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

/// <summary>
/// 飞 / 游鱼区域元数据读取处理器
/// </summary>
public partial class CheepCheepAreaMetaProcessor : ObjectProcessor {
    [Export] public CheepCheepArea.CheepAreaTypeEnum Type { get; set; }

    public override void ProcessObject(Node instance, string metadata) {
        if (instance is not CheepCheepArea area) {
            return;
        }
        // 检查元数据长度
        if (metadata.Length != 9) {
            GD.PushError($"Length of cheep cheep metadata must be 9 characters. metadata string is {metadata}");
            return;
        }
        // 读取末端坐标
        if (!ClassicSmwpCodec.TryDecodeCoordinate(metadata.AsSpan()[..8], out var end)) {
            GD.PushError($"Unknown cheep cheep end coordinate. metadata string is {metadata}");
            return;
        }
        // 读取并检查鱼类型
        int cheepType = metadata[9] - '0';
        if (cheepType is < 0 or > (int)CheepCheepArea.CheepTypeEnum.Spike) {
            GD.PushError($"Unknown cheep type {cheepType}. metadata string is {metadata}");
            return;
        }
        // 赋值
        area.CheepAreaType = Type;
        area.CheepAreaRect = area.CheepAreaRect with { End = end };
        area.CheepType = (CheepCheepArea.CheepTypeEnum)cheepType;
    }
}