using Godot;

namespace SMWP.Level.Data;

/// <summary>
/// 方块数据条目，一个实例代表一种方块。
/// </summary>
[GlobalClass]
public partial class SmwpBlockDatabaseEntry : Resource {
    /// <summary>
    /// Block 在 BlockData 中的 id
    /// </summary>
    [Export] public string Id { get; set; } = "00";
    
    /// <summary>
    /// Block 在模仿者系统中的 id
    /// </summary>
    [Export] public int SerialNumber { get; set; }
    
    [Export] public int TileSource { get; private set; }
    [Export] public Vector2I TileCoord { get; private set; } = Vector2I.Zero;
}