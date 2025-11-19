using System.Diagnostics.CodeAnalysis;
using Godot;
using SMWP.Level.Background;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlDataHolder : Node {
    [Export] public SmwpBlockDatabase BlockDatabase { get; private set; } = null!;
    [Export] public BackgroundDatabase Backgrounds { get; private set; } = null!;

    public bool TryGetBlock(BlockId id, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return ByBlockId.TryGetValue(id, out block);
    }

    public bool TryGetBlockBySerialNumber(int serialNumber, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return BySerialNumber.TryGetValue(serialNumber, out block);
    }

    private System.Collections.Generic.Dictionary<BlockId, SmwpBlockDatabaseEntry> ByBlockId { get; } = [];
    private System.Collections.Generic.Dictionary<int, SmwpBlockDatabaseEntry> BySerialNumber { get; } = [];
    
    public override void _Ready() {
        base._Ready();
        Index();
    }

    private void Index() {
        ByBlockId.Clear();
        foreach (var entry in BlockDatabase.Entries) {
            if (BlockId.TryParse(entry.Id, out var rid)) {
                ByBlockId[rid] = entry;
            }
            if (entry.SerialNumber != 0) {
                BySerialNumber[entry.SerialNumber] = entry;
            }
        }
    }
}