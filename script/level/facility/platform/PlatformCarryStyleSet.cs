using Godot;
using System;

public partial class PlatformCarryStyleSet : Node {
    [Export] private PlatformStyleSet _platformStyleSet = null!;
    [Export] private CollisionShape2D _carryShape2D = null!;
    
    [Export] private GDC.Dictionary<PlatformStyleSet.PlatformStyleEnum, Shape2D> _shapeMapping = null!;
    
    public override void _Ready() {
        if (!_shapeMapping.TryGetValue(_platformStyleSet.PlatformStyle, out var shape)) return;
        _carryShape2D.Shape = shape;
    }
}
