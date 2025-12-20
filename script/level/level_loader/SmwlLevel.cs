using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlLevel : Node2D {
    [ExportGroup("References")]
    
    [Export] public PackedScene LevelTemplate = null!;
    [Export] public TileMapLayer BlocksTilemap { get; private set; } = null!;
    [Export] public SmwlDataHolder DatabaseHolder { get; private set; } = null!;
    
    /// <summary>
    /// 坐标为 32 的整数倍的实心所用的 TileMap
    /// </summary>
    [Export] public TileMapLayer ObstacleTileMap { get; private set; } = null!;
    
    /// <summary>
    /// 坐标为 32 的整数倍的实心所用的 Tile 在 <see cref="ObstacleTileMap"/> 的 TileSet 的第 0 个 TileSource 中的坐标
    /// </summary>
    [Export] public Vector2I ObstacleTileCoord { get; private set; }
    
    [Export] public SmwlLoader SmwlLoader { get; private set; } = null!;
    [Export] public FileDialog OpenSmwlDialog { get; private set; } = null!;

    private Node2D? _levelTemplate;
    private LevelConfig? _levelConfig;
    private ImitatorBuilder _imitatorBuilder;
    
    // Todo: player position set
    private Node2D? _player;

    public override void _Ready() {
        base._Ready();

        if (GameManager.LevelFileStream == null) {
            // 测试用
            // 改成拖拽加载了，更方便（
            GetWindow().FilesDropped += files => OnOpenSmwlDialogFileSelected(files[0]);
        } else {
            OnDataInstallStarted();
            Install(GameManager.LevelFileStream);
        }
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            await using var input = File.OpenRead(file);
            
            OnDataInstallStarted();
            
            if (await SmwlLoader.Load(input) is { } data) {
                GameManager.LevelFileStream = data;
                Install(data);
            } else {
                foreach (var error in SmwlLoader.ErrorMessage) {
                    GD.PrintErr(error);
                }
            }
        } else {
            GD.PrintErr($"File {file} does not exist");
        }
    }

    public void Install(SmwlLevelData data) {
        // 设置文件头数据
        InstallHeader(data.Header);
        // 设置额外设置
        InstallAdditions(data.AdditionalSettings);
        // 安装 Blocks
        InstallBlocks(data);
        // 安装活动对象
        InstallObjects(data.Objects);
        
        // 数据设置结束，实例化关卡模板
        OnDataInstallFinished();
    }

    public void InstallHeader(ClassicSmwlHeaderData header) {
        if (_levelConfig == null) {
            GD.PushError("InstallHeader: _levelConfig is null!");
            return;
        }
        _levelConfig.RoomWidth = header.RoomWidth;
        _levelConfig.RoomHeight = header.RoomHeight;
        _levelConfig.LevelTitle = header.LevelTitle;
        _levelConfig.LevelAuthor = header.LevelAuthor;
        _levelConfig.Time = header.Time;
        _levelConfig.Gravity = header.Gravity;
        _levelConfig.KoopaEnergy = header.KoopaEnergy;
        _levelConfig.WaterHeight = header.WaterHeight;
        _levelConfig.BgpId = header.BgpId;
        _levelConfig.BgmId = header.BgmId;
        
    }

    private void InstallBlocks(SmwlLevelData data) {
        var blocks = data.Blocks;
        var width = blocks.Width;
        var values = blocks.BlockValues;
        int y = 0;
        while (true) {
            var line = values[y];
            for (int x = 0; x < width; x++) {
                var id = new BlockId(line[2 * x], line[2 * x + 1]);
                InstallBlock(new Vector2I(x, y), id);
            }
            ++y;
            if (y >= values.Count) {
                break;
            }
        }
    }

    private void InstallBlock(Vector2I pos, BlockId id) {
        if (DatabaseHolder.TryGetBlock(id, out var block)) {
            BlocksTilemap.SetCell(pos, block.TileSource, block.TileCoord);
        }
    }

    public void InstallObjects(Array<ClassicSmwlObject> objects) {
        // 初始化模仿者 builder。
        _imitatorBuilder = new ImitatorBuilder(DatabaseHolder, BlocksTilemap.TileSet);
        // 安装物品
        foreach (var @object in objects) {
            // 安装需要特殊处理的对象（实心，模仿者等）
            if (InstallSpecialObject(@object)) {
                continue;
            }
            // 安装普通对象
            // 每个对象对应一个 2D 场景。
            InstallRegularObject(@object);
        }
        // 安装合并后的模仿者 TileMap
        _imitatorBuilder.Finish();
        foreach (var tilemap in _imitatorBuilder.FinishedTileMaps) {
            tilemap.CollisionEnabled = false;
            AddChild(tilemap);
        }
        _imitatorBuilder.Clear();
    }

    private bool InstallSpecialObject(ClassicSmwlObject @object) {
        switch (@object.Id) {
            // 实心
            case SpecialObjectIds.Obstacle: {
                var pos = @object.Position;
                if (pos.X % 32 == 0 && pos.Y % 32 == 0) {
                    var tileCoord = (Vector2I)pos / 32;
                    ObstacleTileMap.SetCell(tileCoord, 0, ObstacleTileCoord);
                } else {
                    InstallRegularObject(@object);
                }
                break;
            }
            // 模仿者
            case SpecialObjectIds.Imitator:
                _imitatorBuilder.NextImitator(@object);
                break;
            case SpecialObjectIds.LevelStart:
                if (_player is { } player) {
                    player.GlobalPosition = @object.Position;
                    player.Translate(new Vector2(0, -12));
                }
                break;
            default:
                return false;
        }
        return true;
    }

    private void InstallRegularObject(ClassicSmwlObject @object) {
        // 获取对象数据条目
        if (!DatabaseHolder.TryGetObject(@object.Id, out var definition)) {
            GD.PushWarning($"Unknown object id {@object.Id}");
            return;
        }
        // 生成物品
        if (definition.Prefab is not { } prefab) {
            GD.PushError($"Trying to install regular object ({@object.Id}) with empty prefab");
            return;
        }
        
        var processor = definition.MetadataProcessor;
        var instance = processor?.CreateInstance(definition, @object) ?? prefab.Instantiate();
        
        if (instance is Node2D active) {
            active.GlobalPosition = @object.Position;
            active.Translate(definition.SpawnOffset);
        }
        processor?.ProcessObject(instance, @object);
        
        instance.ResetPhysicsInterpolation();
        _levelTemplate!.AddChild(instance);
    }
    
    public void InstallAdditions(ClassicSmwlAdditionalSettingsData addition) {
        if (_levelConfig == null) {
            GD.PushError("InstallAddition: _levelConfig is null!");
            return;
        }
        _levelConfig.ModifiedMovement = addition.ModifiedMovement;
        _levelConfig.RotoDiscLayer = addition.RotoDiscLayer;
        _levelConfig.LayerOrder = addition.LayerOrder;
        _levelConfig.FluidType = addition.FluidType;
        _levelConfig.AutoFluid = addition.AutoFluid;
        _levelConfig.FluidT1 = addition.FluidT1;
        _levelConfig.FluidT2 = addition.FluidT2;
        _levelConfig.FluidSpeed = addition.FluidSpeed;
        _levelConfig.FluidDelay = addition.FluidDelay;
        _levelConfig.AdvancedSwitch = addition.AdvancedSwitch;
        _levelConfig.FastRetry = addition.FastRetry;
        _levelConfig.MfStyleBeet = addition.MfStyleBeet;
        _levelConfig.CelesteStyleSwitch = addition.CelesteStyleSwitch;
        _levelConfig.MfStylePipeExit = addition.MfStylePipeExit;
        _levelConfig.FasterLevelPass = addition.FasterLevelPass;
        _levelConfig.HUDDisplay = addition.HUDDisplay;
        _levelConfig.RainyLevel = addition.RainyLevel;
        _levelConfig.FallingStarsLevel = addition.FallingStarsLevel;
        _levelConfig.SnowyLevel = addition.SnowyLevel;
        _levelConfig.ThunderLevel = addition.ThunderLevel;
        _levelConfig.WindyLevel = addition.WindyLevel;
        _levelConfig.Darkness = addition.Darkness;
        _levelConfig.Brightness = addition.Brightness;
        _levelConfig.ThwompActivateBlocks = addition.ThwompActivateBlocks;
    }
    
    // 数据开始读取，准备填装数据
    public void OnDataInstallStarted() {
        _levelTemplate = LevelTemplate.Instantiate<Node2D>();
        _levelConfig = _levelTemplate.GetNode<LevelConfig>("LevelConfig");
        _player = _levelTemplate.GetNode<Node2D>("Mario");
    }

    // 数据读取完毕，实例化关卡模版
    public void OnDataInstallFinished() {
        AddChild(_levelTemplate);
    }

    public void JumpToScene(string sceneUid) {
        GetTree().ChangeSceneToFile(sceneUid);
    }
}