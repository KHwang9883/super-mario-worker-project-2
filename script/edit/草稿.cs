using Godot;
using System;
using SMWP.Edit.Command;

public partial class 草稿 : Node {
    public enum EditModeType {
        None,
        PlaceObject,
        EraseObject,
        RotoDisc,
    }
    [Export] public EditModeType CurrentEditMode =  EditModeType.None;
    
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
            switch (CurrentEditMode) {
                case EditModeType.PlaceObject: PlaceObject(); break;
                case EditModeType.EraseObject: EraseObject(); break;
                // TODO: Special Object
                case EditModeType.RotoDisc: break;
            }
        }
    }
    public void PlaceObject() {
        var cmdPlaceObject = new CmdPlaceObject();
        cmdPlaceObject.SpawnerObjectScene = CurrentSpawnerObjectScene;
        Callable.From(() => {
            AddChild(cmdPlaceObject);
        }).CallDeferred();
    }

    public void EraseObject() {
        var cmdEraseObject = new CmdEraseObject();
        Callable.From(() => {
            AddChild(cmdEraseObject);
        }).CallDeferred();
    }
}