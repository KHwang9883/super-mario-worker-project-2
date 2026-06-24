using Godot;
using System;

[GlobalClass]
public partial class SpawnerObject : Node {
    public enum SpawnerEditType {
        Buddy,
        Scenery,
        Mark,
        Bonus,
    }
    [Export] public SpawnerEditType SpawnerType = SpawnerEditType.Buddy;
    
    [Export] public string SpawnerIdStr = "";
    [Export] public PackedScene SpawnScene = null!;
    
    [Export] public Vector2 GridOffset = Vector2.Zero;
    
    [Export] public GDC.Dictionary<string, string>? MetaDict;
}
