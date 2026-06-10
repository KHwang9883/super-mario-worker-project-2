using Godot;
using System;
using System.Linq;
using SMWP.Edit.Command;

public partial class UndoRedoManager : Node {
    [Export] public NodePath PathToUndoButton = "../UndoButton";
    [Export] public NodePath PathToRedoButton = "../RedoButton";
    
    public Button? UndoButton;
    public Button? RedoButton;
    
    public Node? CommandsNode;
    public GDC.Array<Node> Commands = [];

    public int CurrentStep = 0;

    public override void _Ready() {
        UndoButton = GetNode<Button>(PathToUndoButton);
        RedoButton = GetNode<Button>(PathToRedoButton);
        RedoButton.Pressed += OnRedoButtonPressed;
        UndoButton.Pressed += OnUndoButtonPressed;
        
        CommandsNode = GetTree().GetFirstNodeInGroup("commands_node");
        CommandsNode.ChildEnteredTree += OnEditNodeChildrenUpdated;
        
        OnEditNodeChildrenUpdated();
    }
    
    public void OnEditNodeChildrenUpdated(Node child = null!) {
        if (CommandsNode == null) {
            GD.PushError("EditNode is null!");
            return;
        }
        
        Commands = CommandsNode.GetChildren();
        // 存在撤回的情况下，之后有新的编辑操作，删除后面的 cmd 节点
        if (CurrentStep < Commands.Count - 1) {
            var commandsSize = Commands.Count;
            var preRemovingCommands = new GDC.Array<Node>();
            for (int i = 0; i < commandsSize - CurrentStep - 1; i += 1) {
                //Commands[CurrentStep].Free();
                preRemovingCommands.Add(Commands[CurrentStep]);
                Commands.Remove(Commands[CurrentStep]);
            }
            foreach (var node in preRemovingCommands) {
                node.QueueFree();
            }
        }
        CurrentStep = Commands.Count;
        UpdateButtons();
        StatusPrint();
    }

    public void OnUndoButtonPressed() {
        CurrentStep--;
        if (Commands[CurrentStep] is not AbstractCmdEdit lastCmd) {
            GD.PushError("Undo command is null!");
            return;
        }
        lastCmd.Undo();
        UpdateButtons();
        StatusPrint();
    }
    
    public void OnRedoButtonPressed() {
        if (Commands[CurrentStep] is not AbstractCmdEdit lastCmd) {
            GD.PushError("Redo command is null!");
            return;
        }
        lastCmd.Do();
        CurrentStep++;
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
        
        UndoButton.Disabled = CurrentStep < 1;
        RedoButton.Disabled = Commands.Count == CurrentStep;
    }

    public void StatusPrint() {
        GD.Print($"CurrentStep: {CurrentStep}, Commands.Count: {Commands.Count}");
    }
}
