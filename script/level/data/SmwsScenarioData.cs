using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class SmwsScenarioData : Resource {
    [Export] public required ClassicSmwsHeaderData Header { get; set; }
    [Export] public required GDC.Array<SmwlLevelData> Levels { get; set; } = [];
}