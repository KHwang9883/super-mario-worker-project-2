using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit {
    public PackedScene? SpawnerObjectScene;
    public Node2D? SpawnerObject;

    public Vector2 PlaceCursorPosition;

    public override void _EnterTree() {
        base._EnterTree();
        PlaceCursorPosition = CursorPositionProvider.GetCursorPosition(this);
    }

    public override void Do() {
        var position = PlaceCursorPosition;
        var gridPosition = new Vector2I((int)(position.X / 32f) * 32, (int)(position.Y / 32f) * 32);
        if (SpawnerObjectScene == null) {
            GD.PushError("SpawnerObjectScene is null! Can't be placed.");
            return;
        }
		// 放置物品
        // TODO: 放置物品有类型：Block、Buddies...，如果重叠时类型也相同那么不能放置
        SpawnerObject = SpawnerObjectScene.Instantiate<Node2D>();
        var offset = SpawnerObject.GetNode<Marker2D>("EditObjectBase/LeftTopMarker2D").Position;
        SpawnerObject.Position = gridPosition - offset;
        Callable.From(() => { 
            AddChild(SpawnerObject);
            GD.Print($"放置物品在 {gridPosition}, 物品是 {SpawnerObject.GetPath()}");
        }).CallDeferred();
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
