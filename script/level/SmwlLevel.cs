using System;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlLevel : Node2D {
    [Export] public TileMapLayer ObstacleTileMap { get; private set; } = null!;
    [Export] public Vector2I ObstacleTileId { get; private set; }    
    [Export] public PackedScene UnalignedObstaclePrefab { get; set; } = null!;

    public void Install(SmwlLevelData data) {
        foreach (var @object in data.Objects) {
            if (@object.Id == 218) {
                var pos = @object.Position;
                if (pos.X % 32 == 0 && pos.Y % 32 == 0) {
                    var tileCoord = (Vector2I)pos / 32;
                    ObstacleTileMap.SetCell(tileCoord, 0, ObstacleTileId);
                } else {
                    var obstacle = UnalignedObstaclePrefab.Instantiate<Node2D>();
                    obstacle.GlobalPosition = pos;
                    AddChild(obstacle);
                }
            }
        }
    }
}