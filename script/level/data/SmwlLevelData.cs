using Godot;
using Godot.Collections;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class SmwlLevelData : Resource {
    public const int ImitatorId = 142;
    public const int PlayerId = 219;
    
    [Export] public required ClassicSmwlHeaderData Header { get; set; }
    [Export] public required ClassicSmwlAdditionalSettingsData AdditionalSettings { get; set; }
    [Export] public required ClassicSmwlBlocksData Blocks { get; set; }
    [Export] public required Array<ClassicSmwlObject> Objects { get; set; }
    [Export] public required Dictionary<string, string> V2Metadata { get; set; }
}