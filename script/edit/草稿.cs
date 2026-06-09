using Godot;
using System;
using SMWP.Edit.Command;

public partial class 草稿 : Node {
    [Export]
    public PackedScene? CurrentSpawnerObjectScene;

    // 将 _Input 改为 _UnhandledInput
    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed) {
                var position = GetViewport().GetMousePosition();
                var cmdPlaceObject = new CmdPlaceObject();
                cmdPlaceObject.SpawnerObjectScene = CurrentSpawnerObjectScene;
                cmdPlaceObject.PlaceMousePosition = position;
                AddChild(cmdPlaceObject);
            }
        }
    }
}