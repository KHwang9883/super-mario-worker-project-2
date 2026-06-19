using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit {
    public PackedScene? SpawnerObjectScene;
    public Node2D? EditObjectInstance;

    public Vector2 PlaceCursorPosition;

    public override void _EnterTree() {
        base._EnterTree();
        PlaceCursorPosition = CursorPositionProvider.GetCursorPosition(this);
    }

    public override void Do() {
        if (SpawnerObjectScene == null) {
            GD.PushError("SpawnerObjectScene is null! Can't be placed.");
            return;
        }
		// 放置物品
        // TODO: 放置物品有类型：Block、Buddies...，如果重叠时类型也相同那么不能放置
        EditObjectInstance = SpawnerObjectScene.Instantiate<Node2D>();
        var gridOffset = EditObjectInstance.GetNode<SpawnerObject>("EditObjectBase/SpawnerObject").GridOffset;
        var position = PlaceCursorPosition - gridOffset;
        var gridPosition = new Vector2I((int)(position.X / 32f) * 32, (int)(position.Y / 32f) * 32) + gridOffset;
        var offset = EditObjectInstance.GetNode<Marker2D>("EditObjectBase/LeftTopMarker2D").Position;
        EditObjectInstance.Position = gridPosition - offset;
        Callable.From(() => { 
            AddChild(EditObjectInstance);
            GD.Print($"放置物品在 {gridPosition}, 物品是 {EditObjectInstance.GetPath()}");
        }).CallDeferred();
	}
	public override void Undo() {
		// 擦除物品
        if (EditObjectInstance == null) {
            GD.PushError("SpawnerObject is null! Can't undo.");
            return;
        }
        EditObjectInstance.QueueFree();
        GD.Print($"Undo: 擦除物品 {EditObjectInstance.Name}");
	}
}
