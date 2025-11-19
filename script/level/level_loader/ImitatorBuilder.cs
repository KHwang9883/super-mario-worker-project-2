using System.Collections.Generic;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public record struct ImitatorBuilder(
    SmwlDataHolder Database,
    TileSet BlockTileSet
) {
    public List<TileMapLayer> FinishedTileMaps { get; } = [];

    public void NextImitator(ClassicSmwlObject imitator) {
        var pos = (Vector2I)imitator.Position;
        // 不支持非整数坐标的模仿者
        if (imitator.Position != pos) {
            GD.PushWarning($"Imitator with non integer position {imitator.Position}");
            return;
        }
        // 计算偏移，如果遇到了新的偏移不同的模仿者则创建新的 tilemap
        var align = ComputeAlign(pos);
        if (!int.TryParse(imitator.Metadata, out int blockSerialNumber)) {
            GD.PushWarning($"Invalid imitator id: {imitator.Metadata}");
            return;
        }
        if (align != CurrentAlign) {
            if (CurrentTileMap is { } current) {
                FinishedTileMaps.Add(current);
            }
            CurrentAlign = align;
            CurrentTileMap = NewTileMap();
        }
        CurrentTileMap ??= NewTileMap();
        // 把模仿者添加到 tilemap 里
        var tileCoord = (pos - align) / 32;
        AddToCurrentTileMap(tileCoord, blockSerialNumber);
    }

    public void Finish() {
        if (CurrentTileMap is { } current) {
            FinishedTileMaps.Add(current);
        }
        CurrentAlign = null;
        CurrentTileMap = null;
    }

    public void Clear() {
        FinishedTileMaps.Clear();
    }
    
    private Vector2I? CurrentAlign { get; set; }
    private TileMapLayer? CurrentTileMap { get; set; }

    private void AddToCurrentTileMap(Vector2I tileCoord, int blockSerialNumber) {
        if (!Database.TryGetBlockBySerialNumber(blockSerialNumber, out var block)) {
            GD.PushWarning($"Unknown block serial number: {blockSerialNumber}");
            return;
        }
        CurrentTileMap!.SetCell(tileCoord, block.TileSource, block.TileCoord);
    }

    private TileMapLayer NewTileMap() {
        return new TileMapLayer {
            TileSet = BlockTileSet,
        };
    }

    public static Vector2I ComputeAlign(Vector2I position) {
        return new Vector2I(position.X % 32, position.Y % 32);
    }
}