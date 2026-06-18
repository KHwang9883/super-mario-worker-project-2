using Godot;
using System;

public partial class PaintObjectButtonSet : Node {

    [Export] public 草稿.EditModeType PaintMode = 草稿.EditModeType.None;
    [Export] public PackedScene PaintObjectScene { get; set; } = null!;

    public 草稿? EditNode;
    private Button? _paintObjectButton;

    public override void _Ready() {
        _paintObjectButton = GetNode<Button>("..");
        _paintObjectButton.Pressed += OnPaintObjectButtonPressed;
        EditNode = GetTree().GetFirstNodeInGroup("edit_node") as 草稿;
    }

    public void OnPaintObjectButtonPressed() {
        if (EditNode == null) {
            GD.PushError("Edit Node not set!");
            return;
        }
        EditNode.CurrentSpawnerObjectScene = PaintObjectScene;
    }
}
