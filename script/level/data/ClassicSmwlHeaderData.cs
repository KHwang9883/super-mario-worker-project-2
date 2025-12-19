using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 经典 smwl 文件头数据。
/// 存放了解析后的，从文件开头到 BlocksDataStart 之前的数据
/// </summary>
[GlobalClass]
public partial class ClassicSmwlHeaderData : Resource {
    [Export] public int RoomWidth { get; set; } = 1024;
    [Export] public int RoomHeight { get; set; } = 640;
    [Export] public string LevelTitle { get; set; } = "LEVEL 1";
    [Export] public string LevelAuthor { get; set; } = "";
    [Export] public int Time { get; set; } = 600;
    [Export] public float Gravity { get; set; } = 5;
    [Export] public int KoopaEnergy { get; set; } = 5;
    [Export] public float WaterHeight { get; set; } = 800;
    [Export] public int BgpId { get; set; } = 1;
    [Export] public int BgmId { get; set; } = 1;
}