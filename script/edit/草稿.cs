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
    public PackedScene? CurrentSpawnerObjectScene {
        get => _currentSpawnerObjectScene;
        set {
            _currentSpawnerObjectScene = value;
            if (_currentSpawnerObjectScene == null) {
                GD.PushError("SpawnerObjectScene is null!");
                return;
            }
            _cachedEditInstance = _currentSpawnerObjectScene.Instantiate();
            var marker2DNode = _cachedEditInstance.GetNode<Marker2D>("EditObjectBase/LeftTopMarker2D");
            _cachedOffset = marker2DNode.Position;
            var sprite2DNode = _cachedEditInstance.GetNode<Sprite2D>("EditObjectBase/Sprite2D");
            _cachedTexture = sprite2DNode.Texture;
            var spawnerObjectNode = _cachedEditInstance.GetNode<SpawnerObject>("EditObjectBase/SpawnerObject");
            _cachedEditType = spawnerObjectNode.SpawnerType;
            _cachedId = spawnerObjectNode.SpawnerIdStr;
        }
    }

    private PackedScene? _currentSpawnerObjectScene;
    private Node? _cachedEditInstance;
    private Texture2D? _cachedTexture;
    private Vector2 _cachedOffset;
    private SpawnerObject.SpawnerEditType _cachedEditType = SpawnerObject.SpawnerEditType.Buddy;
    private string _cachedId = "";

    [Export]
    public Sprite2D? PlaceObjectSprite2D;
    
    public void PlaceObjectPreview() {
        if (CurrentEditMode is not EditModeType.None and not EditModeType.EraseObject) {
            if (_currentSpawnerObjectScene == null) return;

            if (PlaceObjectSprite2D == null) {
                //GD.PushError("PlaceObjectSprite2D is null!");
                return;
            }
            PlaceObjectSprite2D.Texture = _cachedTexture;
            var cursorPosition = CursorPositionProvider.GetCursorPosition(this);
            var gridPosition = new Vector2((int)(cursorPosition.X / 32f) * 32, (int)(cursorPosition.Y / 32f) * 32);
            PlaceObjectSprite2D.Position = gridPosition - _cachedOffset;
        }
    }

    // 鼠标点击放置物品
    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);

        if (CanPlaceObject()) PlaceObjectPreview();
        
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
        cmdPlaceObject.SpawnerObjectScene = _currentSpawnerObjectScene;
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
        Vector2 cursorWorldPos = CursorPositionProvider.GetCursorPosition(this);
        var space = GetParent<Node2D>().GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = cursorWorldPos;
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = 1 << 16;
        var results = space.IntersectPoint(query);
        if (results.Count == 0) return true;
        foreach (var result in results) {
            if (result.TryGetValue("collider", out var collider)) {
                var spawnerObject = collider.As<Node>().GetNodeOrNull<SpawnerObject>("%SpawnerObject");
                
                if (spawnerObject == null) continue;

                if (spawnerObject.SpawnerType == _cachedEditType) {
                    // 遇到同类型物品检测到重叠不允许放置
                    if (_cachedEditType != SpawnerObject.SpawnerEditType.Mark) return false;
                    
                    // Marks以SpawnerIdStr为单位进行检测
                    if (spawnerObject.SpawnerIdStr == _cachedId) return false;
                }
                
                /*
                if (collider.As<Node>().HasMeta("edit_object")) {
                    GD.Print("Can't place object!");
                    return false;
                }
                */
            }
        }
        return true;
    }
}