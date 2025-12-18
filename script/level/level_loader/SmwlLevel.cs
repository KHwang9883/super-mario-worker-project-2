using System.IO;
using System.Linq;
using Godot;
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
    
    /// <summary>
    /// 坐标不为 32 的整数倍的实心的原型
    /// </summary>
    [Export] public PackedScene UnalignedObstaclePrefab { get; private set; } = null!;
    
    [Export] public SmwlLoader SmwlLoader { get; private set; } = null!;
    [Export] public FileDialog OpenSmwlDialog { get; private set; } = null!;

    private Node2D? _levelTemplate;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        base._Ready();
        // 测试用
        OpenSmwlDialog.FileSelected += OnOpenSmwlDialogFileSelected;
        OpenSmwlDialog.Visible = true;
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            await using var input = File.OpenRead(file);
            
            OnDataInstallStarted();
            
            if (await SmwlLoader.Load(input) is { } data) {
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
        InstallHeader(data.Header);
        InstallBlocks(data);
        ImitatorBuilder imitatorBuilder = new(DatabaseHolder, BlocksTilemap.TileSet);
        foreach (var @object in data.Objects) {
            switch (@object.Id) {
                case 218: {
                    var pos = @object.Position;
                    if (pos.X % 32 == 0 && pos.Y % 32 == 0) {
                        var tileCoord = (Vector2I)pos / 32;
                        ObstacleTileMap.SetCell(tileCoord, 0, ObstacleTileCoord);
                    } else {
                        var obstacle = UnalignedObstaclePrefab.Instantiate<Node2D>();
                        obstacle.GlobalPosition = pos;
                        AddChild(obstacle);
                    }
                    break;
                }
                case SmwlLevelData.ImitatorId:
                    imitatorBuilder.NextImitator(@object);
                    break;
            }
        }
        imitatorBuilder.Finish();
        foreach (var tilemap in imitatorBuilder.FinishedTileMaps) {
            AddChild(tilemap);
        }
        imitatorBuilder.Clear();
        
        OnDataInstallFinished();
    }

    public void InstallHeader(ClassicSmwlHeaderData header) {
        /*var background = DatabaseHolder.Backgrounds.Entries.FirstOrDefault(entry => entry.BackgroundId == header.LevelBackground);
        if (background != null) {
            AddChild(background.BackgroundScene.Instantiate());
        } else {
            GD.PushWarning($"Unknown background id: {header.LevelBackground}");
        }*/
        _levelConfig.RoomWidth = header.Width;
        _levelConfig.RoomHeight = header.Height;
        _levelConfig.LevelTitle = header.Title;
        _levelConfig.LevelAuthor = header.Author;
        _levelConfig.Time = header.LevelTime;
        _levelConfig.Gravity = header.Gravity;
        _levelConfig.KoopaEnergy = header.BossEnergy;
        _levelConfig.WaterHeight = header.WaterLevel;
        _levelConfig.BgpId = header.LevelBackground;
        _levelConfig.BgmId = header.BackgroundMusic;
        
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

    // 数据开始读取，准备填装数据
    public void OnDataInstallStarted() {
        _levelTemplate = LevelTemplate.Instantiate<Node2D>();
        _levelConfig = _levelTemplate.GetNode<LevelConfig>("LevelConfig");
    }

    // 数据读取完毕，实例化关卡模版
    public void OnDataInstallFinished() {
        AddChild(_levelTemplate);
    }
}