using Godot;

namespace SMWP.Level.Data;

public partial class ClassisSmwlObject : Resource {
    [Export] public int Id { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] public string Metadata { get; set; } = "";
}