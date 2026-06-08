using Godot;
using System;

public partial class PaintObjectSet : Node {
    public enum PaintModeType {
        Normal,
        Roto,
    }

    [Export] public PaintModeType PaintMode = PaintModeType.Normal;
    [Export] public PackedScene PaintObjectScene { get; set; } = null!;
}
