using Godot;
using System;

namespace SMWP.Edit.Command;

public partial class CmdPlaceObject : AbstractCmdEdit
{
	public override void Do() {
		var position = GetViewport().GetMousePosition();
		// 放置物品
		GD.Print($"放置物品在 {position}");
	}
	public override void Undo() {
		// 擦除物品
	}
}
