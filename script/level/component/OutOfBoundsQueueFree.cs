using Godot;
using SMWP.Util;

namespace SMWP.Level.Component;

[GlobalClass]
public partial class OutOfBoundsQueueFree : Node
{
    // 出屏销毁
    [ExportGroup("OutOfScreen")]
    [Export] public bool OutOfScreenDetection;
    [Export] public bool ScreenUp;
    [Export] public float ScreenUpOffset = 99999f;
    [Export] public bool ScreenDown = true;
    [Export] public float ScreenDownOffset = 32f;
    [Export] public bool ScreenLeft = true;
    [Export] public float ScreenLeftOffset = 16f;
    [Export] public bool ScreenRight = true;
    [Export] public float ScreenRightOffset = 16f;
    
    // 出房间销毁
    [ExportGroup("OutOfRoom")]
    [Export] public bool OutOfRoomDetection = true;
    [Export] public bool RoomUp;
    [Export] public float RoomUpOffset = 99999f;
    [Export] public bool RoomDown = true;
    [Export] public float RoomDownOffset = 32f;
    [Export] public bool RoomLeft = true;
    [Export] public float RoomLeftOffset = 16f;
    [Export] public bool RoomRight = true;
    [Export] public float RoomRightOffset = 16f;

    private Rect2 _roomBounds;
    private Node2D _target = null!;
    private LevelConfig _levelConfig = null!;

    public override void _Ready() {
        _target = (Node2D)GetParent();
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _roomBounds = new Rect2(Vector2.Zero, new Vector2(_levelConfig.RoomWidth, _levelConfig.RoomHeight));
    }

    public override void _PhysicsProcess(double delta) {
        bool destroy = false;
        
        // 出屏检测（核心修改：按方向偏移量判断）
        if (OutOfScreenDetection) {
            var screenRect = ScreenUtils.GetScreenRect(_target);
            
            if (_target.Position.Y < screenRect.Position.Y - ScreenUpOffset) destroy = true;
            if (_target.Position.Y > screenRect.End.Y + ScreenDownOffset) destroy = true;
            if (_target.Position.X < screenRect.Position.X - ScreenLeftOffset) destroy = true;
            if (_target.Position.X > screenRect.End.X + ScreenRightOffset) destroy = true;
        }

        // 出房间检测（同理，按方向偏移量判断）
        if (OutOfRoomDetection) {
            if (_target.Position.Y < _roomBounds.Position.Y - RoomUpOffset) destroy = true;
            if (_target.Position.Y > _roomBounds.End.Y + RoomDownOffset) destroy = true;
            if (_target.Position.X < _roomBounds.Position.X - RoomLeftOffset) destroy = true;
            if (_target.Position.X > _roomBounds.End.X + RoomRightOffset) destroy = true;
        }

        if (destroy) {
            _target.QueueFree();
        }
    }
}