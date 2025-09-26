using System.IO;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level;

public partial class SmwlLevel : Node2D {
    [ExportGroup("References")]
    [Export] public TileMapLayer ObstacleTileMap { get; private set; } = null!;
    [Export] public Vector2I ObstacleTileId { get; private set; }    
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