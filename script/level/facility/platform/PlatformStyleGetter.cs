using Godot;
using System;

public partial class PlatformStyleGetter : Node {
    [Export] private PlatformStyleSet _platformStyleSet = null!;
    
    private PlatformStyleSet _carrier = null!;
    
    public override void _Ready() {
        _carrier = GetParent<PlatformStyleSet>();
        _carrier.PlatformStyle = _platformStyleSet.PlatformStyle;
    }
}
