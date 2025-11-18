using Godot;
using System;
using Godot.Collections;

[GlobalClass]
public partial class BackgroundDatabaseEntry : Resource {
    [Export] public int BackgroundId { get; set; } = 1;
    [Export] public PackedScene BackgroundScene { get; set; } = null!;
}
