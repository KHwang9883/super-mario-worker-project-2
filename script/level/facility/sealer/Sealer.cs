using Godot;
using System;

[Tool]
public partial class Sealer : StaticBody2D {
    [Export] private PackedScene _singleSolidScene = GD.Load<PackedScene>("uid://d2n4sofwy5eyn");
    [Export] private float _fixedHeight;
    private Node2D? _player;

    public override void _Ready() {
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        Callable.From(() => {
            var singleSolid = _singleSolidScene.Instantiate<Node2D>();
            singleSolid.Position = Position;
            AddSibling(singleSolid);
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        if (Engine.IsEditorHint()) {
            Position = Position with { Y = _fixedHeight };
            return;
        }
        if (_player == null) return;
        Position = Position with { Y = Mathf.Min(_fixedHeight, _player.Position.Y) };
    }
}
