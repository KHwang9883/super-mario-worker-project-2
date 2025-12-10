using Godot;
using System;
using SMWP.Level.Block;

public partial class MessageBlock : BlockHit {
    private AnimatedSprite2D? _ani;
    private static float _frameProgress;

    public override void _Ready() {
        base._Ready();
        if (Sprite is AnimatedSprite2D ani) {
            _ani = ani;
        }
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        // Todo: 待测试
        // 记录非被顶状态开关砖当前动画进度，保持所有开关砖同步
        if (_ani != null && _ani.Animation.Equals("hit")) {
            _frameProgress = _ani.FrameProgress;
        }
    }

    public override void OnBlockHit(Node2D collider) {
        base.OnBlockHit(collider);
        _ani?.Play("hit");
    }
    protected override void OnBumped() {
        base.OnBumped();
        if (_ani == null) return;
        _ani.Play("default");
        _ani.FrameProgress = _frameProgress;
    }
}
