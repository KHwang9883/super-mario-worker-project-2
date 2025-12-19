using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlDataHolder : Node {
    [Export] public SmwpBlockDatabase BlockDatabase { get; private set; } = null!;
    
    /// <summary>
    /// 支持使用多个数据库，
    /// 这样就可以对每种对象大类都做一个单独的数据库了，更方便管理了。
    /// </summary>
    [Export] public SmwpObjectDatabase[] ObjectDatabases { get; private set; } = null!;

    public bool TryGetBlock(BlockId id, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return ByBlockId.TryGetValue(id, out block);
    }

    public bool TryGetBlockBySerialNumber(int serialNumber, [NotNullWhen(true)] out SmwpBlockDatabaseEntry? block) {
        return BySerialNumber.TryGetValue(serialNumber, out block);
    }

    public bool TryGetObject(int id, [NotNullWhen(true)] out SmwpObjectDatabaseEntry? @object) {
        return ObjectById.TryGetValue(id, out @object);
    }

    private Dictionary<BlockId, SmwpBlockDatabaseEntry> ByBlockId { get; } = [];
    private Dictionary<int, SmwpBlockDatabaseEntry> BySerialNumber { get; } = [];
    private Dictionary<int, SmwpObjectDatabaseEntry> ObjectById { get; } = [];
    
    public override void _Ready() {
        base._Ready();
        Index();
    }

    private void Index() {
        // 索引方块
        ByBlockId.Clear();
        foreach (var entry in BlockDatabase.Entries) {
            if (BlockId.TryParse(entry.Id, out var rid)) {
                ByBlockId[rid] = entry;
            }
            if (entry.SerialNumber != 0) {
                BySerialNumber[entry.SerialNumber] = entry;
            }
        }
        // 索引活动对象
        foreach (var db in ObjectDatabases) {
            foreach (var entry in db.Entries) {
                if (entry is not null) {
                    ObjectById[entry.Id] = entry;   
                }
            }
        }
    }
}