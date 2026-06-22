using Godot;
using System;

public partial class LevelLoad : Node {
    [ExportCategory("Ref")]
    [Export] public FileDialog DialogWindow = null!;
    
    [Export] public LevelSettings Settings { get; set; } = null!;
    [Export] public TileMapLayer BlocksTileMapLayer { get; set; } = null!;
    [Export] public Node2D ObjectNode2D { get; set; } = null!;
    
    public string CurrentPath = "user://Untitled Level.smwl";
    
    
    public override void _Ready() {
        base._Ready();
        DialogWindow.Confirmed += OnFileDialogConfirmed;
        DialogWindow.Confirmed += LoadLevel;
    }
    
    public void LoadLevel() {
        // TODO: Load level
    }
    
    public void OnFileDialogConfirmed() {
        CurrentPath = DialogWindow.CurrentFile;
    }
}
