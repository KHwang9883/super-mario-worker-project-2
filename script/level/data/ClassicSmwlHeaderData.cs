using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlHeaderData : Resource {
    [Export] public int Width { get; set; } = 1024;
    [Export] public int Height { get; set; } = 640;
    [Export] public string Title { get; set; } = "LEVEL 1";
    [Export] public string Author { get; set; } = "";
    [Export] public int LevelTime { get; set; } = 600;
    [Export] public float Gravity { get; set; } = 5;
    [Export] public int BossEnergy { get; set; } = 5;
    [Export] public float WaterLevel { get; set; } = 800;
    [Export] public int LevelBackground { get; set; } = 1;
    [Export] public int BackgroundMusic { get; set; } = 1;
}