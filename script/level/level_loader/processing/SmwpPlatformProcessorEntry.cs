using Godot;

namespace SMWP.Level.Loader.Processing;

[GlobalClass]
public partial class SmwpPlatformProcessorEntry : Resource {
    [Export] public PlatformProcessor.PlatformType Type { get; set; }
    [Export] public PlatformStyleSet.PlatformStyleEnum Style { get; set; }
    [Export] public float Speed { get; set; }
}