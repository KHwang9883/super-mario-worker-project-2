using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwsHeaderData : Resource {
    [Export] public int Lives { get; set; }
}