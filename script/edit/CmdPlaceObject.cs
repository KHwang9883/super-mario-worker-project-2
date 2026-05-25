using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit
{
	private Node2D? _cursorPositionProvider;

	public override void Do() {
		var position = _cursorPositionProvider!.GlobalPosition;
		// 放置物品
	}
	public override void Undo() {
		// 擦除物品
	}
}
