using Godot;
using System;
using SMWP.Edit.Command;

public partial class CmdEraseObject : AbstractCmdEdit {
    public SpawnerObject.SpawnerEditType SpawnerObjectType;
    public string SpawnerObjectId = "";
    public Vector2 ErasePosition;
    public GDC.Array<Node2D> ErasedNodes = [];
    public Node? ObjectsNode2D;

    public override void _EnterTree() {
        base._EnterTree();
        ObjectsNode2D = GetTree().GetFirstNodeInGroup("objects_node_2d");
    }

    public override void Do() {
        // 检测鼠标位置获取这些编辑物品Node
        // 根据Type和DeleteMode决定删除的物品类型
        ErasedNodes = GetObjectNodes(ErasePosition);
        
        if (ObjectsNode2D == null) {
            GD.PushError("objects_node_2d not found!");
            return;
        }
        
        // 擦除物品的本质并不是QueueFree，而是移出SceneTree
        foreach (var node in ErasedNodes) {
            ObjectsNode2D.RemoveChild(node);
        }
    }

    public override void Undo() {
        if (ObjectsNode2D == null) {
            GD.PushError("objects_node_2d not found!");
            return;
        }
        foreach (var node in ErasedNodes) {
            if (node is not { } node2D) continue;
            
            ObjectsNode2D.AddChild(node2D);
        }
    }

    public GDC.Array<Node2D> GetObjectNodes(Vector2 position) {
        var editObjNodes = new GDC.Array<Node2D>();
        
        var space = GetNode<Node2D>("../..").GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = position;
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1 << 16;
        var results = space.IntersectPoint(query);
        if (results.Count == 0) {
            GD.PushError("Nothing to erase!");
            return editObjNodes;
        }
        foreach (var result in results) {
            if (result.TryGetValue("collider", out var collider)) {
                var editArea2D = collider.As<Node2D>();
                var spawnerObject = editArea2D.GetNodeOrNull<SpawnerObject>("%SpawnerObject");
                var editNode2D = editArea2D.GetNode<Node2D>("../..");
                
                if (spawnerObject == null) continue;

                if (spawnerObject.SpawnerType == SpawnerObjectType) {
                    // 遇到同类型物品
                    if (SpawnerObjectType != SpawnerObject.SpawnerEditType.Mark) {
                        editObjNodes.Add(editNode2D);
                        continue;
                    }
                    
                    // Marks以SpawnerIdStr为单位进行检测
                    if (spawnerObject.SpawnerIdStr == SpawnerObjectId) {
                        editObjNodes.Add(editNode2D);
                        continue;
                    }
                }
            }
        }
        if (editObjNodes.Count == 0) GD.PushError("Nothing to erase!");
        
        return editObjNodes;
    }
}
