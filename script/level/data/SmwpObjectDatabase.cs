using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 对象数据库，存放了对象数据条目。
/// </summary>
[GlobalClass]
public partial class SmwpObjectDatabase : Resource {
    [Export] public SmwpObjectDatabaseEntry?[] Entries { get; set; } = [];
}