using Godot;
using SMWP.Edit.Command;

public partial class EditManager : Node {
    [Signal]
    public delegate void CurrentSpawnerChangedEventHandler();
    
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
            var sprite2DNode = _cachedEditInstance.GetNode<Sprite2D>("EditObjectBase/Sprite2D");
            _cachedTexture = sprite2DNode.Texture;
            _cachedFlipV = sprite2DNode.FlipV;
            _cachedScale = sprite2DNode.Scale;
            var marker2DNode = _cachedEditInstance.GetNode<Marker2D>("EditObjectBase/LeftTopMarker2D");
            _cachedSpriteOffset = sprite2DNode.Position + sprite2DNode.Offset;
            _cachedOffset = marker2DNode.Position - _cachedSpriteOffset;
            var spawnerObjectNode = _cachedEditInstance.GetNode<SpawnerObject>("EditObjectBase/SpawnerObject");
            _cachedEditType = spawnerObjectNode.SpawnerType;
            _cachedId = spawnerObjectNode.SpawnerIdStr;
            _cachedGridOffset = spawnerObjectNode.GridOffset;
            EmitSignal(SignalName.CurrentSpawnerChanged);
        }
    }

    private PackedScene? _currentSpawnerObjectScene;
    private Node? _cachedEditInstance;
    private Texture2D? _cachedTexture;
    private Vector2 _cachedOffset;
    private SpawnerObject.SpawnerEditType _cachedEditType = SpawnerObject.SpawnerEditType.Buddy;
    private string _cachedId = "";
    private Vector2 _cachedGridOffset;
    private bool _cachedFlipV;
    private Vector2 _cachedPlacePosition;
    private Vector2 _cachedSpriteOffset;
    private Vector2 _cachedScale;

    [Export] private Node _commandNode = null!;

    [Export] public Sprite2D? PlaceObjectSprite2D;
    
    public void PlaceObjectPreview() {
        if (CurrentEditMode is not EditModeType.None and not EditModeType.EraseObject) {
            if (_currentSpawnerObjectScene == null) return;

            if (PlaceObjectSprite2D == null) {
                //GD.PushError("PlaceObjectSprite2D is null!");
                return;
            }
            PlaceObjectSprite2D.Texture = _cachedTexture;
            var cursorPosition = CursorPositionProvider.GetCursorPosition(this) - _cachedGridOffset;
            var gridPosition = new Vector2((int)(cursorPosition.X / 32f) * 32, (int)(cursorPosition.Y / 32f) * 32) + _cachedGridOffset;
            _cachedPlacePosition = gridPosition - _cachedOffset;
            PlaceObjectSprite2D.Position = _cachedPlacePosition;
            PlaceObjectSprite2D.FlipV = _cachedFlipV;
            PlaceObjectSprite2D.Scale = _cachedScale;
        }
    }

    // 鼠标点击放置物品
    public override void _Process(double delta) {
        base._Process(delta);

        // 悬停在 Control 节点上不允许操作
        if (GetViewport().GuiGetHoveredControl() != null) return;
        
        if (CanPlaceObject()) PlaceObjectPreview();
        
        // TODO: 右键擦除物品（需要特别检查不在特别放置模式下）
        /*
        if (@event.IsActionPressed("erase_object")) {
            EraseObject();
        }
        */
        // 放置物品
        if (IsButtonPressed("place_object")) {
            switch (CurrentEditMode) {
                case EditModeType.PlaceObject:
                    if (!CanPlaceObject()) return;
                    PlaceObject();
                    break;
                // TODO: Special Object
                case EditModeType.RotoDisc:
                    if (!CanPlaceObject()) return;
                    break;
            }
        }
        if (IsButtonPressed("erase_object")) {
            if (CurrentEditMode is EditModeType.PlaceObject or EditModeType.EraseObject) {
                CurrentEditMode = EditModeType.EraseObject;
                EraseObject();
            }
        } else {
            if (CurrentEditMode == EditModeType.EraseObject) CurrentEditMode = EditModeType.PlaceObject;
        }
    }
    public void PlaceObject() {
        var cmdPlaceObject = new CmdPlaceObject();
        cmdPlaceObject.SpawnerObjectScene = _currentSpawnerObjectScene;
        cmdPlaceObject.PlacePosition = _cachedPlacePosition - _cachedSpriteOffset;
        Callable.From(() => {
            _commandNode.AddChild(cmdPlaceObject);
        }).CallDeferred();
    }

    public void EraseObject() {
        var cmdEraseObject = new CmdEraseObject();
        cmdEraseObject.SpawnerObjectType = _cachedEditType;
        cmdEraseObject.SpawnerObjectId = _cachedId;
        cmdEraseObject.ErasePosition = CursorPositionProvider.GetCursorPosition(this);
        Callable.From(() => {
            _commandNode.AddChild(cmdEraseObject);
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
            }
        }
        return true;
    }

    public bool IsButtonPressed(string button) {
        return Input.IsActionPressed(button);
    }

    public bool IsButtonJustPressed(string button) {
        return Input.IsActionJustPressed(button);
    }
}