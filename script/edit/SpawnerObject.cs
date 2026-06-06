using Godot;
using System;

[GlobalClass]
public partial class SpawnerObject : Node {
    [Export] public string SpawnerName = "";
    [Export] public PackedScene SpawnScene = null!;
}
