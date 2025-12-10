using Godot;
using System;
using SMWP.Level.Block;

public partial class MessageBlock : BlockHit {
    [Export] private string _message = "";
    private bool _isShown;
    
    private AnimatedSprite2D? _ani;
    private static float _frameProgress;
    private static int _frame;
    
    private MessageDisplay? _messageDisplay;

    public override void _Ready() {
        base._Ready();
        
        if (Sprite is AnimatedSprite2D ani) {
            _ani = ani;
        }

        _message = StringProcess.ConvertHashAndNewline(_message);
        
        if (GetTree().GetFirstNodeInGroup("message_display") is MessageDisplay messageDisplay)
            _messageDisplay = messageDisplay;
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        
        // 记录非被顶状态开关砖当前动画进度，保持所有开关砖同步
        if (_ani == null) return;
        if (_ani.Animation.Equals("hit")) return;
        _frameProgress = _ani.FrameProgress;
        _frame = _ani.Frame;
    }

    protected override bool IsBumpable(Node2D collider) {
        _ani?.Play("hit");
        if (_messageDisplay == null) {
            GD.PushError($"{this}: _messageDisplay is null!");
            return false;
        }
        _messageDisplay.SetMessage(_message, _isShown, this);
        Bumpable = false;
        return true;
    }
    protected override void OnBumped() {
        base.OnBumped();
        if (_ani == null) return;
        _ani.Play("default");
        _ani.Frame = _frame;
        _ani.FrameProgress = _frameProgress;
        Bumpable = true;
    }

    public void SetShown() {
        _isShown = true;
    }
}
