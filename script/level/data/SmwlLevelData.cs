using Godot;
using Godot.Collections;

namespace SMWP.Level.Data;

/// <summary>
/// 代表一个解析完成后的 smwl 关卡数据。
/// </summary>
[GlobalClass]
public partial class SmwlLevelData : Resource {
    [Export] public required ClassicSmwlHeaderData Header { get; set; }
    [Export] public required ClassicSmwlAdditionalSettingsData AdditionalSettings { get; set; }
    [Export] public required ClassicSmwlBlocksData Blocks { get; set; }
    [Export] public required Array<ClassicSmwlObject> Objects { get; set; }
    [Export] public required Dictionary<string, string> V2Metadata { get; set; }
}