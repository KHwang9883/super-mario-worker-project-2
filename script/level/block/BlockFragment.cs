using Godot;
using System;
using Godot.Collections;

namespace SMWP.Level.Block.Brick;

public partial class BlockFragment : Node {
    [Signal]
    public delegate void BlockFragmentStartedEventHandler();
    
    [Export] private Node2D? _parent;
    [Export] private PackedScene _blockFragmentScene = null!;

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
    
    public override void _Ready() {
        _parent ??= GetParent<Node2D>();
    }
    public void Create() {
        if (_parent == null) return;
        EmitSignal(SignalName.BlockFragmentStarted);
        for (var i = 0; i < _fragmentVelocityData.Count; i++) {
            var blockFragment = _blockFragmentScene.Instantiate<BrickFragment>();
            _parent.AddSibling(blockFragment);
            blockFragment.GlobalPosition =
                new Vector2(_parent.GlobalPosition.X + _fragmentCreatePosition[i].X,
                    _parent.GlobalPosition.Y + _fragmentCreatePosition[i].Y);
            blockFragment.ResetPhysicsInterpolation();
            blockFragment.SpeedX = _fragmentVelocityData[i].X;
            blockFragment.SpeedY = _fragmentVelocityData[i].Y;
        }
    }
}
