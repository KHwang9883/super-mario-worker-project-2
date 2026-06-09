using Godot;
using System;
using SMWP.Edit.Command;

public partial class UndoRedoManager : Node {
    [Export] public NodePath PathToUndoButton = "../UndoButton";
    [Export] public NodePath PathToRedoButton = "../RedoButton";
    
    public Button? UndoButton;
    public Button? RedoButton;
    
    public Node? EditNode;
    public GDC.Array<Node> Commands = [];

    public int CurrentStep = 0;

    public override void _Ready() {
        UndoButton = GetNode<Button>(PathToUndoButton);
        RedoButton = GetNode<Button>(PathToRedoButton);
        RedoButton.Pressed += OnRedoButtonPressed;
        UndoButton.Pressed += OnUndoButtonPressed;
        
        EditNode = GetTree().GetFirstNodeInGroup("edit_node");
        EditNode.ChildEnteredTree += OnEditNodeChildrenUpdated;
        EditNode.ChildExitingTree += OnEditNodeChildrenUpdated;
        
        OnEditNodeChildrenUpdated();
    }
    
    public void OnEditNodeChildrenUpdated(Node child = null!) {
        if (EditNode == null) {
            GD.PushError("EditNode is null!");
            return;
        }
        
        if (child is not AbstractCmdEdit) return;

        Commands = EditNode.GetChildren();
        // 存在撤回的情况下，之后有新的编辑操作，删除后面的 cmd 节点
        if (CurrentStep < Commands.Count - 1) {
            var commandsSize = Commands.Count;
            for (int i = CurrentStep + 1; i < commandsSize; i++) {
                Commands[i].QueueFree();
            }
        }
        CurrentStep = Commands.Count - 1;
        UpdateButtons();
        StatusPrint();
    }

    public void OnUndoButtonPressed() {
        if (Commands[CurrentStep] is not AbstractCmdEdit lastCmd) {
            GD.PushError("Undo command is null!");
            return;
        }
        lastCmd.Undo();
        CurrentStep--;
        UpdateButtons();
        StatusPrint();
    }
    
    public void OnRedoButtonPressed() {
        CurrentStep++;
        if (Commands[CurrentStep] is not AbstractCmdEdit lastCmd) {
            GD.PushError("Redo command is null!");
            return;
        }
        lastCmd.Do();
        GD.Print("Redo!");
        UpdateButtons();
        StatusPrint();
    }

    public void UpdateButtons() {
        if (UndoButton == null) {
            GD.PushError("UndoButton is null!");
            return;
        }
        if (RedoButton == null) {
            GD.PushError("RedoButton is null!");
            return;
        }
        
        UndoButton.Disabled = CurrentStep < 0;
        RedoButton.Disabled = Commands.Count - 1 == CurrentStep;
    }

    public void StatusPrint() {
        GD.Print($"CurrentStep: {CurrentStep}, Commands.Count - 1: {Commands.Count - 1}");
    }
}
