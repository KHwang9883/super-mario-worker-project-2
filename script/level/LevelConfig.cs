using Godot;
using System;
using System.Linq;
using Godot.Collections;
using SMWP.Level.Background;
using FileAccess = Godot.FileAccess;

namespace SMWP.Level;

[GlobalClass]
public partial class LevelConfig : Node {
    [Signal]
    public delegate void GamePausedEventHandler();
    [Signal]
    public delegate void GameResumedEventHandler();
    
    [Signal]
    public delegate void SwitchSwitchedEventHandler(SwitchTypeEnum switchType);
    
    [ExportGroup("BasicLevelSettings")]
    [Export] public float RoomWidth = 1024f;
    [Export] public float RoomHeight = 640f;
    
    [Export] public string LevelTitle = "LEVEL 1";
    [Export] public string LevelAuthor = "";
    [Export] public int Time = 600;
    [Export] public float Gravity = 5f;
    [Export] public int KoopaEnergy = 5;
    [Export] public float WaterHeight = 800f;
    [Export] public int BgpId = 1;
    [Export] public int BgmId = 1;

    [ExportGroup("AdditionalSettings")]
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
    
    // Todo: 发光物体格式定义（二期工程、补丁和三期工程？）
    //[Export] public ... ???

    [Export] public bool ThwompActivateBlocks;

    // Todo: SmwpVersion 兼容性处理 
    public int SmwpVersion = 2000;
    
    [ExportGroup("Database")]
    [Export] public BackgroundDatabase BgpDatabase { get; private set; } = null!;
    [Export] public BgmDatabase BgmDatabase { get; private set; } = null!;
    
    private PackedScene? _backgroundScene;
    private BackgroundSet? _backgroundSet;
    
    // 开关砖
    public enum SwitchTypeEnum {
        Red,
        Yellow,
        Green,
        Cyan,
        Blue,
        Magenta,
        Kohl,
        White,
    }

    public Godot.Collections.Dictionary<SwitchTypeEnum, bool> Switches { get; private set; } = null!;

    // 关卡初始化
    public override void _Ready() {
        // Room Size 初始化见 LevelCamera
        
        // Time Set
        GameManager.SetLevelTime(Time);
        
        // Set Player
        GameManager.Player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        
        // Water Height Set
        //GD.Print($"IsCheckpointWaterHeightRecorded: {GameManager.IsCheckpointWaterHeightRecorded}");
        var water = (Water)GetTree().GetFirstNodeInGroup("water_global");
        if (GameManager.IsCheckpointWaterHeightRecorded) {
            water.Position = water.Position with { Y = GameManager.CheckpointWaterHeight };
            //GD.Print($"Water Position Y: {water.Position.Y}");
        } else {
            water.Position = water.Position with { Y = WaterHeight };
        }
        
        // Background Set
        SetBgp(BgpId);
        
        // Bgm 初始化见 BgmPlayer
        
        // Faster Level Pass Set
        GameManager.IsFasterLevelPass = FasterLevelPass;
        
        // 初始化开关砖
        Switches = new Godot.Collections.Dictionary<SwitchTypeEnum, bool>();
        
        foreach (SwitchTypeEnum type in Enum.GetValues(typeof(SwitchTypeEnum))) {
            Switches.Add(type, false); 
        }
        
        // 红色开关砖第二功能
        SwitchSwitched += OnRedSwitchSwitched;
    }

    public override void _PhysicsProcess(double delta) {
        if (Input.IsActionJustPressed("pause") && GetTree().Paused) {
            SetResume();
            EmitSignal(SignalName.GameResumed);
        } else if (Input.IsActionJustPressed("pause") && !GetTree().Paused || GameManager.GamePause) { 
            GetTree().Paused = true;
            GameManager.GamePause = true;
            EmitSignal(SignalName.GamePaused);
        }

        if (Input.IsActionJustPressed("confirm") && GameManager.GamePause) {
            QuitLevel();
        }
    }

    public void SetResume() {
        GetTree().Paused = false;
        GameManager.GamePause = false;
    }

    public void QuitLevel() {
        GameManager.GameOverClear();
        var gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
        gameManager.JumpToLevel();
    }
    
    public void SetBgm(int bgmId) {
        BgmId = bgmId;
        GetNode<BgmPlayer>("BgmPlayer").SetBgm(true);
    }
    public void SetBgp(int bgpId) {
        BgpId = bgpId;
        
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
    
    // 切换开关砖
    public void ToggleSwitch(SwitchTypeEnum type, bool isOn, bool whiteAdv = false, bool kohlAdv = false) {
        if (!Switches.ContainsKey(type)) return;
        if (!kohlAdv) {
            Switches[type] = isOn;
        }

        if (!AdvancedSwitch) return;
        //GD.Print($"{this}: Advanced Switch Switched!");
        if (whiteAdv) return;
        EmitSignal(SignalName.SwitchSwitched, Variant.From(type));
    }

    // 红色开关砖第二功能
    public void OnRedSwitchSwitched(SwitchTypeEnum type) {
        if (type != SwitchTypeEnum.Red) return;
        switch (FluidType) {
            case Fluid.FluidTypeEnum.Water:
                FluidType = Fluid.FluidTypeEnum.Lava;
                break;
            case Fluid.FluidTypeEnum.Lava:
                FluidType = Fluid.FluidTypeEnum.Water;
                break;
        }
    }
}
