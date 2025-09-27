using Godot;

namespace SMWP.Level.Data;

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