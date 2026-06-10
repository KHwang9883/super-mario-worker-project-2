using Godot;
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

    [Export] public Sprite2D? PlaceObjectSprite2D;

    // 缓存：CurrentSpawnerObjectScene 变更时更新，避免每帧遍历场景状态
    private PackedScene? _cachedScene;
    private Texture2D? _cachedTexture;
    private Vector2 _cachedOffset;
    
    private void UpdateCache() {
        if (_cachedScene == CurrentSpawnerObjectScene) return;
        _cachedScene = CurrentSpawnerObjectScene;
        if (_cachedScene == null) {
            _cachedTexture = null;
            _cachedOffset = Vector2.Zero;
            return;
        }
        _cachedTexture = GetPropertyWithoutInstantiate<Texture2D>(_cachedScene, "Sprite2D", "texture");
        _cachedOffset = GetPropertyWithoutInstantiate<Vector2>(_cachedScene, "LeftTopMarker2D", "position");
    }
    
    public void PlaceObjectPreview() {
        if (CurrentEditMode is not EditModeType.None and not EditModeType.EraseObject) {
            if (CurrentSpawnerObjectScene == null) return;

            if (PlaceObjectSprite2D == null) {
                //GD.PushError("PlaceObjectSprite2D is null!");
                return;
            }
            UpdateCache();
            PlaceObjectSprite2D.Texture = _cachedTexture;
            var mousePosition = GetViewport().GetMousePosition();
            var gridPosition = new Vector2((int)(mousePosition.X / 32f) * 32, (int)(mousePosition.Y / 32f) * 32);
            PlaceObjectSprite2D.Position = gridPosition - _cachedOffset;
        }
    }

    // 不实例化获取场景内节点的属性（含递归搜索实例化子场景）
    public static T GetPropertyWithoutInstantiate<[MustBeVariant] T>(PackedScene scene, string nodeName, string propertyNameGds) {
        var result = TryGetPropertyInScene<T>(scene, nodeName, propertyNameGds);
        if (result.Found) return result.Value;

        GD.PushError($"在场景中未找到节点 '{nodeName}' 的属性 '{propertyNameGds}'");
        return default!;
    }

    private static (bool Found, T Value) TryGetPropertyInScene<[MustBeVariant] T>(PackedScene scene, string nodeName, string propertyNameGds) {
        var state = scene.GetState();
        for (int i = 0; i < state.GetNodeCount(); i++) {
            if (state.GetNodeName(i) == nodeName) {
                for (int j = 0; j < state.GetNodePropertyCount(i); j++) {
                    var propName = state.GetNodePropertyName(i, j);
                    if (propName == propertyNameGds) {
                        var variant = state.GetNodePropertyValue(i, j);
                        if (variant.As<T>() is T result)
                            return (true, result);
                        else {
                            GD.PushError($"属性 '{propertyNameGds}' 的值不能转换为类型 {typeof(T)}");
                            return (false, default!);
                        }
                    }
                }
                // 原场景没有设置属性但是实例化场景中的节点设置了属性，继续搜索其他可能
            }

            // 递归搜索实例化的子场景
            var instancedScene = state.GetNodeInstance(i);
            if (instancedScene != null) {
                var subResult = TryGetPropertyInScene<T>(instancedScene, nodeName, propertyNameGds);
                if (subResult.Found) return subResult;
            }
        }
        return (false, default!);
    }

    // 鼠标点击放置物品
    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);

        PlaceObjectPreview();
        
        // TODO: 右键擦除物品（需要特别检查不在特别放置模式下）
        /*
        if (@event.IsActionPressed("erase_object")) {
            EraseObject();
        }
        */
        // 放置物品
        if (@event.IsActionPressed("place_object")) {
            switch (CurrentEditMode) {
                case EditModeType.PlaceObject:
                    if (!CanPlaceObject()) return;
                    PlaceObject();
                    break;
                case EditModeType.EraseObject:
                    EraseObject();
                    break;
                // TODO: Special Object
                case EditModeType.RotoDisc:
                    if (!CanPlaceObject()) return;
                    break;
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

    public bool CanPlaceObject() {
        Vector2 mouseWorldPos = GetViewport().GetMousePosition();
        var space = GetParent<Node2D>().GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = mouseWorldPos;
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        var results = space.IntersectPoint(query);
        if (results.Count == 0) return true;
        foreach (var result in results) {
            if (result.TryGetValue("collider", out var collider)) {
                if (collider.As<Node>().HasMeta("edit_object")) {
                    GD.Print("Can't place object!");
                    return false;
                }
            }
        }
        return true;
    }
}