using Godot;
using Godot.Collections;
using SMWP.Level.Block.Brick;
using SMWP.Level.Interface;

namespace SMWP.Level.Block;

[GlobalClass]
public partial class BlockHit : Node, IBlockHittable {
    [Signal]
    public delegate void BlockBumpEventHandler();
    [Signal]
    public delegate void BlockBumpEndedEventHandler();
    [Signal]
    public delegate void BlockBreakEventHandler();

    // 顶砖
    [Export] public bool Bumpable;
    [Export] public bool BumpableOneShot;
    [Export] public bool Hidden;
    [Export] private PackedScene _blockBumpArea2DScene = GD.Load<PackedScene>("uid://c14kue38e0gnl");
    [Export] private Node2D? _sprite;
    public enum BumpState {
        Idle,
        Rising,
        Falling,
    }
    private BumpState _bumpState = BumpState.Idle;
    private int _bumpStateTimer;
    protected bool Bumping;

    // 碎砖
    [Export] private bool _breakable;
    [Export] private PackedScene _blockFragmentScene = GD.Load<PackedScene>("uid://duh8768n6ynbd");
    private readonly Array<Vector2> _fragmentCreatePosition = [
        new (-8f, -8f),
        new (8f, 8f),
        new (-8f, 8f),
        new (8f, -8f),
    ];
    private readonly Array<Vector2> _fragmentVelocityData = [
        new (-3f, -6f),
        new (-2f, -4f),
        new (2f, -4f),
        new (3f, -6f),
    ];

    protected StaticBody2D? Parent;
    
    // 隐藏砖设置
    [Export] private CollisionShape2D _collisionShape2D = null!;
    private static Shape2D? _originalCollisionShape2D;
    private uint _originCollisionLayer;
    private RectangleShape2D _hiddenShape = GD.Load<RectangleShape2D>("uid://dgpgao4212wvq");
    
    public override void _Ready() {
        Parent = GetParent<StaticBody2D>();
        // 忘记设置精灵节点，尝试获取
        _sprite ??= (Node2D)GetParent().GetNodeOrNull("Sprite2D");
        _sprite ??= (Node2D)GetParent().GetNodeOrNull("AnimatedSprite2D");
        
        MetadataInject(Parent);

        // 隐藏砖
        _originCollisionLayer = Parent.CollisionLayer;
        _originalCollisionShape2D = (Shape2D)_collisionShape2D.Shape.Duplicate();
        if (!Hidden) return;
        SetHidden();
    }
    public void MetadataInject(Node2D parent) {
        parent.SetMeta("InteractionWithBlock", this);
    }
    
    public override void _PhysicsProcess(double delta) {
        if (_sprite == null) {
            GD.PushError("BlockBump: _sprite is not assigned!");
            return;
        } 
        // 被顶动画
        switch (_bumpState) {
            case BumpState.Rising when _bumpStateTimer < 10:
                _bumpStateTimer++;
                break;
            case BumpState.Rising:
                _bumpState = BumpState.Falling;
                break;
            case BumpState.Falling when _bumpStateTimer > 0:
                _bumpStateTimer--;
                break;
            case BumpState.Falling:
                _bumpState = BumpState.Idle;
                EmitSignal(SignalName.BlockBumpEnded);
                OnBumped();
                break;
        }
        
        if (Parent == null) {
            GD.PushError($"{this}: Parent is null!");
            return;
        }
        
        _sprite.GlobalPosition = new Vector2(
            Parent.GlobalPosition.X, Parent.GlobalPosition.Y - _bumpStateTimer * 2);
    }
    
    public virtual void OnBlockHit(Node2D collider) {
        if (!Bumping && IsBumpable(collider)) {
            Bumping = true;
            EmitSignal(SignalName.BlockBump);
            OnBlockBump();
        }
        if (IsBreakable(collider)) {
            EmitSignal(SignalName.BlockBreak);
            OnBlockBreak();
        }
    }
    
    protected virtual bool IsHittable(Node2D collider) {
        return true;
    }
    protected virtual bool IsBumpable(Node2D collider) {
        return Bumpable;
    }
    protected virtual bool IsBreakable(Node2D collider) {
        return _breakable;
    }
    
    protected virtual void OnBlockBump() {
        if (_bumpState != BumpState.Idle) return;
        _bumpState = BumpState.Rising;
        _bumpStateTimer = 0;
        
        // 隐藏砖显现
        if (Hidden) SetVisible();
        
        // 顶砖判定生成
        Callable.From(() => {
            if (Parent == null) {
                GD.PushError($"{this}: Parent is null!");
                return;
            }
            
            var blockBumpArea2D = _blockBumpArea2DScene.Instantiate<Area2D>();
            blockBumpArea2D.Position = Parent.Position;
            Parent.AddSibling(blockBumpArea2D);
        }).CallDeferred();
    }
    protected virtual void OnBumped() {
        Bumping = false;
        if (BumpableOneShot) Bumpable = false;
    }
    protected virtual void OnBlockBreak() {
        if (Parent == null) {
            GD.PushError($"{this}: Parent is null!");
            return;
        }
        
        for (var i = 0; i < _fragmentVelocityData.Count; i++) {
            var blockFragment = _blockFragmentScene.Instantiate<BlockFragment>();
            Parent.AddSibling(blockFragment);
            blockFragment.GlobalPosition =
                new Vector2(Parent.GlobalPosition.X + _fragmentCreatePosition[i].X,
                    Parent.GlobalPosition.Y + _fragmentCreatePosition[i].Y);
            blockFragment.ResetPhysicsInterpolation();
            blockFragment.SpeedX = _fragmentVelocityData[i].X;
            blockFragment.SpeedY = _fragmentVelocityData[i].Y;
        }
        
        // 碎砖也触发生成顶砖判定
        Callable.From(() => {
            var blockBumpArea2D = _blockBumpArea2DScene.Instantiate<Area2D>();
            blockBumpArea2D.Position = Parent.Position;
            Parent.AddSibling(blockBumpArea2D);
        }).CallDeferred();
        
        Parent.QueueFree();
    }

    public void SetHidden() {
        if (Parent == null) {
            GD.PushError($"{this}: Parent is null!");
            return;
        }
        Parent.Visible = false;
        _collisionShape2D.Position = Vector2.Down * 13f;
        Parent.CollisionLayer = 2;
        _collisionShape2D.Shape = _hiddenShape;
        _collisionShape2D.OneWayCollision = true;
    }
    public void SetVisible() {
        if (Parent == null) {
            GD.PushError($"{this}: Parent is null!");
            return;
        }
        Hidden = false;
        Parent.Visible = true;
        Parent.CollisionLayer = _originCollisionLayer;
        _collisionShape2D.Shape = _originalCollisionShape2D;
        _collisionShape2D.Position = Vector2.Zero;
        _collisionShape2D.OneWayCollision = false;
    }
}