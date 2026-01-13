using Godot;
using System;

public partial class LoadFileButtonDisabled : Node {
    [Export] private NodePath _buttonPath = "..";
    [Export] private NodePath _fileDialogPath = "../../../OpenFile";
    private Button? _button;
    private FileDialog? _fileDialog;

    public override void _Ready() {
        _button = GetNode<Button>(_buttonPath);
        _fileDialog = GetNode<FileDialog>(_fileDialogPath);
        _fileDialog.CloseRequested += SetEnabled;
    }
    
    public void SetEnabled() {
        _button!.Disabled = false;
    }
}
