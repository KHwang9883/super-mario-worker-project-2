using Godot;
using System;

public partial class PlatformStyleOjbkect : Node {
    [Export] private GDC.Dictionary<PlatformStyleSet.PlatformStyleEnum, Shape2D> _shapeMapping = null!;
}
