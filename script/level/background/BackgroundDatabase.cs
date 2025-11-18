using Godot;

namespace SMWP.Level.Background;

[GlobalClass]
public partial class BackgroundDatabase : Resource {
    [Export] public BackgroundDatabaseEntry[] Entries { get; set; } = null!;
}