using Godot;
using System;

namespace SMWP.Level.Block.Brick;

public partial class BlockBump : Node {
    [Signal]
    public delegate void BlockBumpEndedEventHandler();
    [Export] private Node2D? _sprite;
    [Export] private PackedScene _blockBumpArea2DScene = GD.Load<PackedScene>("uid://c14kue38e0gnl");
    
    private Node2D _parent = null!;
    
    public enum BrickState
    {
        Idle,
        Rising,
        Falling,
    }
    private BrickState _brickState = BrickState.Idle;
    private int _brickStateTimer;
    
    public override void _Ready() {
        _parent = (Node2D)GetParent();
        _sprite ??= (Node2D)GetParent().GetNode("Sprite2D");
    }
    public override void _PhysicsProcess(double delta) {
        if (_sprite == null) {
            GD.PushError("BlockDump: _sprite is not assigned!");
            return;
        } 
        
        // 被顶动画
        switch (_brickState) {
            case BrickState.Rising when _brickStateTimer < 10:
                _brickStateTimer++;
                break;
            case BrickState.Rising:
                _brickState = BrickState.Falling;
                break;
            case BrickState.Falling when _brickStateTimer > 0:
                _brickStateTimer--;
                break;
            case BrickState.Falling:
                _brickState = BrickState.Idle;
                //GetTree().Paused = true;
                EmitSignal(SignalName.BlockBumpEnded);
                break;
        }
        
        _sprite.GlobalPosition = new Vector2(
            _parent.GlobalPosition.X, _parent.GlobalPosition.Y - _brickStateTimer * 2);
    }
    public void OnBlockBump() {
        if (_brickState != BrickState.Idle) return;
        _brickState = BrickState.Rising;
        _brickStateTimer = 0;
        
        // 顶砖判定生成
        var blockFragment = _blockBumpArea2DScene.Instantiate<Area2D>();
        blockFragment.Position = _parent.Position;
        _parent.AddSibling(blockFragment);
    }
}
