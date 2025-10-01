using System.IO;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlLevel : Node2D {
    [ExportGroup("References")]
    [Export] public TileMapLayer BlocksTilemap { get; private set; } = null!;
    [Export] public SmwlBlockDataHolder BlocksDatabase { get; private set; } = null!;
    
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

    public override void _Ready() {
        base._Ready();
        // 测试用
        OpenSmwlDialog.FileSelected += OnOpenSmwlDialogFileSelected;
        OpenSmwlDialog.Visible = true;
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            await using var input = File.OpenRead(file);
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
        InstallBlocks(data);
        foreach (var @object in data.Objects) {
            if (@object.Id == 218) {
                var pos = @object.Position;
                if (pos.X % 32 == 0 && pos.Y % 32 == 0) {
                    var tileCoord = (Vector2I)pos / 32;
                    ObstacleTileMap.SetCell(tileCoord, 0, ObstacleTileCoord);
                } else {
                    var obstacle = UnalignedObstaclePrefab.Instantiate<Node2D>();
                    obstacle.GlobalPosition = pos;
                    AddChild(obstacle);
                }
            }
        }
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
        if (BlocksDatabase.TryGetBlock(id, out var block)) {
            BlocksTilemap.SetCell(pos, block.TileSource, block.TileCoord);
        }
    }
}