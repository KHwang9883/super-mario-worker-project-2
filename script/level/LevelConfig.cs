using Godot;
using System;
using Godot.Collections;
using SMWP.Level.Background;
using SMWP.Level.Score;

namespace SMWP.Level;

[GlobalClass]
public partial class LevelConfig : Node {
    [Export] public float RoomWidth = 1024f;
    [Export] public float RoomHeight = 640f;
    [Export] public int Time = 600;
    [Export] public int BackgroundId = 5;
    private PackedScene? _backgroundScene;
    
    [Export] public BackgroundDatabase Database { get; private set; } = null!;

    public override void _Ready() {
        // Time Set
        LevelManager.SetLevelTime(Time);
        
        // Set Player
        LevelManager.Player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        
        // Background Set
        foreach (var entry in Database.Entries) {
            if (entry.BackgroundId != BackgroundId) continue;
            _backgroundScene = entry.BackgroundScene;
            break;
        }

        Callable.From(() => {
            if (_backgroundScene == null) return;
            var background = _backgroundScene.Instantiate<BackgroundSet>();
            AddSibling(background);
        }).CallDeferred();
    }
}
