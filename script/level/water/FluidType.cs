using Godot;
using System;

[GlobalClass]
public partial class SmwpFluidType : Node {
    public enum FluidTypeEnum { Water, Lava }

    [Export] public FluidTypeEnum FluidType = FluidTypeEnum.Water;
    
    public override void _Ready() {
        GetNode("..").SetMeta("FluidType", this);
    }
}
