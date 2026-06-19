using Godot;
using System;

public partial class LevelLoad : Node {
    [ExportCategory("Ref")]
    [Export] public LevelSettings Settings { get; set; } = null!;
    [Export] public TileMapLayer BlocksTileMapLayer { get; set; } = null!;
    [Export] public Node2D ObjectNode2D { get; set; } = null!;
    
    public string CurrentFilePath = "user://";
    
    public void LoadLevel() {
        // TODO: Load level
    }
}
