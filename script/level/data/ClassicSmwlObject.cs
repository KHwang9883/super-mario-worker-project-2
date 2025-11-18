using Godot;

namespace SMWP.Level.Data;

public partial class ClassicSmwlObject : Resource {
    [Export] public int Id { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] public string Metadata { get; set; } = "";
}