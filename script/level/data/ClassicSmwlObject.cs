using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlObject : Resource {
    [Export] public int Id { get; set; }
    [Export] public PackedScene Object { get; set; } = null!;
    public Vector2 Position { get; set; }
    public string Metadata { get; set; } = "";
}