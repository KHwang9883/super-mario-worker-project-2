using Godot;
using System;

namespace SMWP.Edit.Command;


public abstract partial class AbstractCmdEdit : Node {
    public override void _Ready() {
        base._Ready();
		Do();
    }

	public abstract void Do();
	public abstract void Undo();
}
