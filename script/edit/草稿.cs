using Godot;
using System;
using SMWP.Edit.Command;

public partial class 草稿 : Node {
    // 测试用[Export]，后续应当删除
    [Export]
    public PackedScene? CurrentSpawnerObjectScene;
    
    // 鼠标点击放置物品
    /*
    public override void _Process(double delta) {
        base._Process(delta);
        if (Input.IsActionJustPressed("place_object")) {
            PlaceObject();
        }
    }
    */

    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);
        if (@event.IsActionPressed("place_object")) {
            PlaceObject(); 
        }
    }
    public void PlaceObject() {
        var position = GetViewport().GetMousePosition();
        var cmdPlaceObject = new CmdPlaceObject();
        cmdPlaceObject.SpawnerObjectScene = CurrentSpawnerObjectScene;
        cmdPlaceObject.PlaceMousePosition = position;
        Callable.From(() => {
            AddChild(cmdPlaceObject);
        }).CallDeferred();
    }
}