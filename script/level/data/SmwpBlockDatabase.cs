using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 方块数据库，存放了方块数据条目。
/// </summary>
[GlobalClass]
public partial class SmwpBlockDatabase : Resource {
    [Export] public SmwpBlockDatabaseEntry[] Entries { get; private set; } = [];
}