using Godot;
using System;

public partial class PaintObjectButtonSet : Node {

    [Export] public EditManager.EditModeType PaintMode = EditManager.EditModeType.None;
    [Export] public PackedScene PaintObjectScene { get; set; } = null!;

    public EditManager? EditNode;
    private Button? _paintObjectButton;

    public override void _Ready() {
        _paintObjectButton = GetNode<Button>("..");
        _paintObjectButton.Pressed += OnPaintObjectButtonPressed;
        EditNode = GetTree().GetFirstNodeInGroup("edit_node") as EditManager;
    }

    public void OnPaintObjectButtonPressed() {
        if (EditNode == null) {
            GD.PushError("Edit Node not set!");
            return;
        }
        EditNode.CurrentSpawnerObjectScene = PaintObjectScene;
    }
}
