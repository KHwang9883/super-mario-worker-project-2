using Godot;

namespace SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlAdditionalSettingsData : Resource {
    [Export] public bool ModifiedMovement = true;
    [Export] public bool RotoDiscLayer;
    [Export] public LevelConfig.LayerOrderEnum LayerOrder = LevelConfig.LayerOrderEnum.Modified;
    [Export] public Fluid.FluidTypeEnum FluidType = Fluid.FluidTypeEnum.Water;
    [Export] public bool AutoFluid;
    [Export] public float FluidT1;
    [Export] public float FluidT2 = -64;
    [Export(PropertyHint.Range, "0, 9, 1")] public float FluidSpeed = 1f;
    [Export] public int FluidDelay;
    [Export] public bool AdvancedSwitch;
    [Export] public bool FastRetry;
    [Export] public bool MfStyleBeet = true;
    [Export] public bool CelesteStyleSwitch;
    [Export] public bool MfStylePipeExit;
    
    [Export] public bool FasterLevelPass;
    [Export] public bool HUDDisplay = true;
    
    [Export(PropertyHint.Range,"0, 5, 1")] public int RainyLevel;
    [Export(PropertyHint.Range,"0, 3, 1")] public int FallingStarsLevel;
    [Export(PropertyHint.Range,"0, 5, 1")] public int SnowyLevel;
    [Export(PropertyHint.Range,"0, 1, 1")] public int ThunderLevel;
    [Export(PropertyHint.Range,"0, 3, 1")] public int WindyLevel;
    [Export(PropertyHint.Range,"0, 9, 1")] public int Darkness;
    [Export(PropertyHint.Range,"0, 5, 1")] public int Brightness;
    
    [Export] public string Smwp1LightObjectString =
        "0000000000000000000000000000000000000000000000000000000000000000000";

    [Export] public bool ThwompActivateBlocks;

    [Export] public int SmwpVersion;
}
