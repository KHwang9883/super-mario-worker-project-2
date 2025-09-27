using System.Collections.Generic;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlBlockHolder : Node {
    [Export] public SmwpBlockDatabase Database { get; private set; } = null!;

    public Dictionary<BlockId, SmwpBlockDatabaseEntry> ByBlockId { get; } = [];
    
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
        }
    }
}