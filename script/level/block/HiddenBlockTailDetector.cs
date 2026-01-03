using Godot;
using System;
using SMWP.Level.Block;
using SMWP.Level.Projectile.Player;

public partial class HiddenBlockTailDetector : Area2D {
    [Export] private BlockHit? _hiddenBlock;

    public override void _Ready() {
        if (_hiddenBlock == null) {
            var parent = GetParent();
            GD.PushError($"{parent.Name}: HiddenBlockTailDetector: _hiddenBlock is null!");
            return;
        }
        
        if (!_hiddenBlock.Hidden) {
            Callable.From(QueueFree).CallDeferred();
        }
    }

    public void OnBodyEntered(Node2D body) {
        if (body is not Tail) return;
        _hiddenBlock!.OnBlockHit(body);
    }
}
