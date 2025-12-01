using Godot;
using System;
using System.IO;
using Godot.Collections;
using SMWP.Level.Background;
using SMWP.Level.Score;
using FileAccess = Godot.FileAccess;

namespace SMWP.Level;

[GlobalClass]
public partial class LevelConfig : Node {
    [ExportGroup("BasicLevelSettings")]
    [Export] public float RoomWidth = 1024f;
    [Export] public float RoomHeight = 640f;
    
    [Export] public string LevelTitle = "LEVEL 1";
    [Export] public string LevelAuthor = "";
    [Export] public int Time = 600;
    [Export] public float Gravity = 5f;
    // Todo
    [Export] public int KoopaEnergy = 5;
    [Export] public float WaterHeight = 800f;
    [Export] public int BgpId = 1;
    [Export] public int BgmId = 1;

    [ExportGroup("AdditionalSettings")]
    // Todo: 默认为 true，后续若有 bug / 特性复现可以使用这个变量
    [Export] public bool ModifiedMovement = true;
    [Export] public bool RotoDiscLayer;

    public enum LayerOrderEnum { Classic, WaterAbove, Modified }
    // Todo: 读取时确定 Z Index
    [Export] public LayerOrderEnum LayerOrder = LayerOrderEnum.Modified;
    [Export] public Fluid.FluidTypeEnum FluidType = Fluid.FluidTypeEnum.Water;
    [Export] public bool AutoFluid;
    [Export] public float FluidT1;
    [Export] public float FluidT2 = -64;
    [Export(PropertyHint.Range, "0, 9, 1")] public float FluidSpeed = 1f;
    [Export] public int FluidDelay;
    // Todo
    [Export] public bool AdvancedSwitch;
    [Export] public bool FastRetry;
    // Todo
    [Export] public bool MfStyleBeet = true;
    // Todo
    [Export] public bool CelesteStyleSwitch;
    // Todo
    [Export] public bool MfStylePipeExit;
    // Todo
    [Export] public bool FasterLevelPass;
    [Export] public bool HUDDisplay = true;
    
    [Export(PropertyHint.Range,"0, 5, 1")] public int RainyLevel;
    [Export(PropertyHint.Range,"0, 3, 1")] public int FallingStarsLevel;
    [Export(PropertyHint.Range,"0, 5, 1")] public int SnowyLevel;
    [Export(PropertyHint.Range,"0, 1, 1")] public int ThunderLevel;
    [Export(PropertyHint.Range,"0, 3, 1")] public int WindyLevel;
    
    [Export(PropertyHint.Range,"0, 9, 1")] public int Darkness;
    [Export(PropertyHint.Range,"0, 5, 1")] public int Brightness;
    
    // Todo: 发光物体格式定义（二期工程、补丁和三期工程？）
    //[Export] public ... ???

    // Todo
    [Export] public bool ThwompActivateBlocks;
    
    [ExportGroup("Database")]
    [Export] public BackgroundDatabase BgpDatabase { get; private set; } = null!;
    [Export] public BgmDatabase BgmDatabase { get; private set; } = null!;
    
    private PackedScene? _backgroundScene;
    private BackgroundSet? _backgroundSet;
    public override void _Ready() {
        // Time Set
        LevelManager.SetLevelTime(Time);
        
        // Set Player
        LevelManager.Player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        
        // Water Height Set
        var water = (Water)GetTree().GetFirstNodeInGroup("water_global");
        water.Position = water.Position with { Y = WaterHeight };
        
        // Background Set
        SetBgp(BgpId);
        
        // Bgm 初始化见 BgmPlayer
    }

    public void SetBgm(int bgmId) {
        BgmId = bgmId;
        GetNode<BgmPlayer>("BgmPlayer").SetBgm(true);
    }
    public void SetBgp(int bgpId) {
        foreach (var entry in BgpDatabase.Entries) {
            if (entry.BackgroundId != bgpId) continue;
            _backgroundScene = entry.BackgroundScene;
            break;
        }

        // 删除旧背景
        if (_backgroundSet != null) _backgroundSet.Free();
        
        Callable.From(() => {
            if (_backgroundScene == null) return;
            _backgroundSet = _backgroundScene.Instantiate<BackgroundSet>();
            AddSibling(_backgroundSet);
        }).CallDeferred();
    }
    public void SetWaterHeight(float waterHeight) {
        var globalWater = (Water)GetTree().GetFirstNodeInGroup("water_global");
        globalWater.SetWaterHeight(waterHeight);
    }
}
