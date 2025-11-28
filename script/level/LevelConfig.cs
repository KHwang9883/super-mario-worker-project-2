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
    [Export] public int BackgroundId = 5;
    [Export] public int BgmId = 1;

    [ExportGroup("AdditionalSettings")]
    // Todo: 默认为 true，后续若有 bug / 特性复现可以使用这个变量
    [Export] public bool ModifiedMovement = true;
    [Export] public bool RotoDiscLayer;

    public enum LayerOrderEnum { Classic, WaterAbove, Modified }
    // Todo: 读取时确定 Z Index
    [Export] public LayerOrderEnum LayerOrder = LayerOrderEnum.Modified;
    // Todo
    [Export] public Water.FluidTypeEnum FluidType = Water.FluidTypeEnum.Water;
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
    
    // Todo
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
    public override void _Ready() {
        // Time Set
        LevelManager.SetLevelTime(Time);
        
        // Set Player
        LevelManager.Player = (Node2D)GetTree().GetFirstNodeInGroup("player");
        
        // Background Set
        foreach (var entry in BgpDatabase.Entries) {
            if (entry.BackgroundId != BackgroundId) continue;
            _backgroundScene = entry.BackgroundScene;
            break;
        }

        Callable.From(() => {
            if (_backgroundScene == null) return;
            var background = _backgroundScene.Instantiate<BackgroundSet>();
            AddSibling(background);
        }).CallDeferred();
        
        // Bgm Set
        foreach (var entry in BgmDatabase.Entries) {
            if (entry.BgmId != BgmId) continue;
            var bgmPlayer = GetNode<AudioStreamPlayer>("BgmPlayer");
            
            // 首先获取外置的覆盖 BGM
            foreach (var fileName in entry.FileNameForOverride) {
                // 遍历所有的兼容性文件名
                var baseDir = Path.GetDirectoryName(OS.GetExecutablePath());
                baseDir = baseDir?.Replace("\\", "/");
                var bgmPath =
                    baseDir + "/Data/" + entry.AlbumPath + "/" + fileName;
                // 不保留文件后缀，因为之前 SMWP 版本后缀过于混乱，无法进行识别，故强制统一为 OGG 格式
                bgmPath = bgmPath.GetBaseName();
                bgmPath += ".ogg";
                //GD.Print(bgmPath);
                if (Godot.FileAccess.FileExists(bgmPath)) {
                    bgmPlayer.Stream = AudioStreamOggVorbis.LoadFromFile(bgmPath);
                    //GD.Print(bgmPlayer.Stream);
                } else {
                    break;
                }
                //if (bgmPlayer.Stream != null) break;
            }
            
            // 如果没有外置覆盖 BGM 文件则使用内置 BGM
            bgmPlayer.Stream ??= entry.DefaultBgm;
            
            //GD.Print(bgmPlayer.Stream);
            break;
        }
    }
}
