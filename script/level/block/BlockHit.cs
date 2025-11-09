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
    [Export] private bool _bumpable;
    [Export] private PackedScene _blockBumpArea2DScene = GD.Load<PackedScene>("uid://c14kue38e0gnl");
    [Export] private Node2D? _sprite;
    public enum BumpState {
        Idle,
        Rising,
        Falling,
    }
    private BumpState _bumpState = BumpState.Idle;
    private int _bumpStateTimer;
    protected bool Bump;

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

    protected Node2D Parent = null!;
    
    public override void _Ready() {
        Parent = GetParent<Node2D>();
        // 忘记设置精灵节点，尝试获取
        _sprite ??= (Node2D)GetParent().GetNodeOrNull("Sprite2D");
        _sprite ??= (Node2D)GetParent().GetNodeOrNull("AnimatedSprite2D");
        
        MetadataInject(Parent);
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
                //GetTree().Paused = true;
                EmitSignal(SignalName.BlockBumpEnded);
                OnBumped();
                break;
        }
        _sprite.GlobalPosition = new Vector2(
            Parent.GlobalPosition.X, Parent.GlobalPosition.Y - _bumpStateTimer * 2);
    }
    
    public virtual void OnBlockHit(Node2D collider) {
        if (!Bump && IsBumpable(collider)) {
            Bump = true;
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
        return _bumpable;
    }
    protected virtual bool IsBreakable(Node2D collider) {
        return _breakable;
    }
    
    protected virtual void OnBlockBump() {
        if (_bumpState != BumpState.Idle) return;
        _bumpState = BumpState.Rising;
        _bumpStateTimer = 0;
        
        // 顶砖判定生成
        var blockBumpArea2D = _blockBumpArea2DScene.Instantiate<Area2D>();
        blockBumpArea2D.Position = Parent.Position;
        Parent.AddSibling(blockBumpArea2D);
    }
    protected virtual void OnBumped() {
        Bump = false;
    }
    protected virtual void OnBlockBreak() {
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
        Parent.QueueFree();
    }
}