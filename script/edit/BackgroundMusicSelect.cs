using Godot;
using System;

public partial class BackgroundMusicSelect : Control {
    [Signal]
    public delegate void PageSwitchedEventHandler();
    
    private GDC.Array<Node>? _bgmPages;

    public override void _Ready() {
        _bgmPages = GetTree().GetNodesInGroup("bgm_select_page");
    }

    public void OnPageSwitched(Control targetPage) {
        if (targetPage.Visible) return;
        if (_bgmPages != null) {
            foreach (var node in _bgmPages) {
                if (node is Control control) {
                    control.Visible = false;
                }
            }
        }
        targetPage.Visible = true;
        EmitSignal(SignalName.PageSwitched);
    }
}
