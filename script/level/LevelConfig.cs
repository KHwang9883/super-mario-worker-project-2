using Godot;
using System;
using SMWP.Level.Score;

namespace SMWP.Level;

[GlobalClass]
public partial class LevelConfig : Node {
    [Export] public float RoomWidth = 1024f;
    [Export] public float RoomHeight = 768f;
    [Export] public int Time = 600;

    public override void _Ready() {
        LevelManager.Time = Time;
    }
}
