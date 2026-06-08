using Godot;
using System;

[GlobalClass]
public partial class SpawnerObject : Node {
    [Export] public string SpawnerIdStr = "";
    [Export] public PackedScene SpawnScene = null!;

    [Export] public GDC.Dictionary<string, string> MetaDict = null!;
}
