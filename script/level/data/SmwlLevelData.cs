using Godot;
using Godot.Collections;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class SmwlLevelData : Resource {
    [Export] public required ClassicSmwlHeaderData Header { get; set; }
    [Export] public required ClassicSmwlBlocksData Blocks { get; set; }
    [Export] public required Array<ClassisSmwlObject> Objects { get; set; }
    [Export] public required Dictionary<string, string> V2Metadata { get; set; }
}