using Godot;
using System;

[GlobalClass]
public partial class BgmDatabase : Resource {
    [Export] public BgmDatabaseEntry[] Entries { get; set; } = null!;
}
