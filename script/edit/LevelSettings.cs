using Godot;
using System;
using SMWP.Level;

public partial class LevelSettings : Node {
    [Export] public int SmwpVersion = 2000;
    private static readonly GDC.Dictionary<int, string> VersionNames = new()
    {
        [2000] = "v2.0.0-a",
        // 新版本添加示例
        //[2001] = "v2.0.0-b",
        //[2002] = "v2.0.0-rc1",
    };

    [Export] public int LevelSizeX = 640;
    [Export] public int LevelSizeY = 480;
    [Export] public string LevelName = "LEVEL 1";
    [Export] public string LevelAuthor = "";
    [Export] public int Time = 600;
    [Export] public float Gravity = 5f;
    [Export] public int KoopaEnergy = 5;
    [Export] public int LiquidHeight = 800;
    [Export] public int LevelBgId = 1;
    [Export] public int LevelBgmId = 1;

    [Export] public bool ModifiedMovement = true;
    [Export] public bool RotoDiscLayer = false;
    [Export] public LevelConfig.LayerOrderEnum LayerOrder = LevelConfig.LayerOrderEnum.Modified;
    [Export] public bool AutoFluid = false;
    [Export] public float FluidT1 = 0f;
    [Export] public float FluidT2 = -64f;
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

    // TODO: 是时候改成ObjId了
    [Export] public string Smwp1LightObjectString =
        "0000000000000000000000000000000000000000000000000000000000000000000";

    [Export] public bool ThwompActivateBlocks;
}
