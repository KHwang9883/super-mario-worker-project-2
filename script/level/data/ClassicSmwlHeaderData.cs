using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlHeaderData : Resource {
    [Export] public int Width { get; set; }
    [Export] public int Height { get; set; }
    [Export] public string Title { get; set; } = "LEVEL 1";
    [Export] public string Author { get; set; } = "";
    [Export] public float LevelTime { get; set; } = 400;
    [Export] public float Gravity { get; set; } = 5;
    [Export] public int BossEnergy { get; set; } = 5;
    [Export] public float WaterLevel { get; set; } = 500;
    [Export] public int LevelBackground { get; set; }
    [Export] public int BackgroundMusic { get; set; }   
}