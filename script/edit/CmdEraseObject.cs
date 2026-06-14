using Godot;
using System;
using SMWP.Edit.Command;

public partial class CmdEraseObject : AbstractCmdEdit {
    public Vector2 EraseCursorPosition;
    public GDC.Array<Node2D> ErasedNodes = [];
    
    public override void _EnterTree() {
        base._EnterTree();
        EraseCursorPosition = CursorPositionProvider.GetCursorPosition(this);
    }

    public override void Do() {
        // TODO: 检测鼠标位置获取这些编辑物品Node
        // TODO: 根据Type和DeleteMode决定删除的物品类型
        ErasedNodes.Add(null!);
    }

    public override void Undo() {
        foreach (var node in ErasedNodes) {
            if (node is not Node2D node2D) continue;
            
            
        }
    }
}
