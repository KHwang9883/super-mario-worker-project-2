using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit {
    public PackedScene? SpawnerObjectScene;
    public Node2D? EditObjectInstance;
    public Vector2 PlacePosition;

    public override void Do() {
        if (SpawnerObjectScene == null) {
            GD.PushError("SpawnerObjectScene is null! Can't be placed.");
            return;
        }
		// 放置物品
        EditObjectInstance = SpawnerObjectScene.Instantiate<Node2D>();
        EditObjectInstance.Position = PlacePosition;
        Callable.From(() => {
            var objNode = GetTree().GetFirstNodeInGroup("objects_node_2d");
            if (objNode == null) {
                GD.PushError("objects_node_2d not found!");
                return;
            }
            objNode.AddChild(EditObjectInstance);
            GD.Print($"放置物品在 {PlacePosition}, 物品是 {EditObjectInstance.GetPath()}");
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
