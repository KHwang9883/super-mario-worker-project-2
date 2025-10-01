using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlBlockDataHolder : Node {
    [Export] public SmwpBlockDatabase Database { get; private set; } = null!;

    public bool TryGetBlock(BlockId id, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return ByBlockId.TryGetValue(id, out block);
    }

    public bool TryGetBlockBySerialNumber(int serialNumber, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return BySerialNumber.TryGetValue(serialNumber, out block);
    }

    private Dictionary<BlockId, SmwpBlockDatabaseEntry> ByBlockId { get; } = [];
    private Dictionary<int, SmwpBlockDatabaseEntry> BySerialNumber { get; } = [];
    
    public override void _Ready() {
        base._Ready();
        Index();
    }

    private void Index() {
        ByBlockId.Clear();
        foreach (var entry in Database.Entries) {
            if (BlockId.TryParse(entry.Id, out var rid)) {
                ByBlockId[rid] = entry;
            }
            if (entry.SerialNumber != 0) {
                BySerialNumber[entry.SerialNumber] = entry;
            }
        }
    }
}