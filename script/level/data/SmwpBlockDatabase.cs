using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class SmwpBlockDatabase : Resource {
    [Export] public SmwpBlockDatabaseEntry[] Entries { get; private set; } = [];
}