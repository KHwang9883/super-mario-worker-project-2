using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit {
    public PackedScene? SpawnerObjectScene;
    public Node2D? SpawnerObject;

    public Vector2 PlaceMousePosition;

    public override void Do() {
        var position = PlaceMousePosition;
        var gridPosition = new Vector2I((int)(position.X / 32f) * 32, (int)(position.Y / 32f) * 32);
        if (SpawnerObjectScene == null) {
            GD.PushError("SpawnerObjectScene is null! Can't be placed.");
            return;
        }
		// 放置物品
        SpawnerObject = SpawnerObjectScene.Instantiate<Node2D>();
        var offset = SpawnerObject.GetNode<Marker2D>("EditObjectBase/LeftTopMarker2D").Position;
        SpawnerObject.Position = gridPosition - offset;
        AddChild(SpawnerObject);
		GD.Print($"放置物品在 {gridPosition}");
	}
	public override void Undo() {
		// 擦除物品
        if (SpawnerObject == null) {
            GD.PushError("SpawnerObject is null! Can't undo.");
            return;
        }
        SpawnerObject.QueueFree();
        GD.Print($"Undo: 擦除物品 {SpawnerObject.Name}");
	}
}
