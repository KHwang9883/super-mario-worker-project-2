using Godot;
using System;

public partial class PaintObjectButtonSet : Node {

    [Export] public 草稿.EditModeType PaintMode = 草稿.EditModeType.None;
    [Export] public PackedScene PaintObjectScene { get; set; } = null!;
}
