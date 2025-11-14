using Godot;
using System;

public partial class ThwompMovement : Node {
    [Signal]
    public delegate void ThwompBlockHitEventHandler();
    [Signal]
    public delegate void PlaySoundStunEventHandler();
    [Signal]
    public delegate void PlaySoundTauntEventHandler();
    [Export] private float _triggerDistance = 80f;
    [Export] private float _gravity = 1f;
    [Export] private int _groundedTime = 96;
    [Export] private float _reserveSpeed = 1f;
    private CharacterBody2D? _parent;
    private Node2D? _player;
    private float _originY;
    private float _speedY;
    private int _groundedTimer;

    public enum ThwompState {
        Ready,
        Falling,
        Grounded,
        Reserve,
    }
    public ThwompState CurrentState = ThwompState.Ready;

    public override void _Ready() {
        _parent ??= (CharacterBody2D)GetParent();
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _originY = _parent.Position.Y;
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;

        switch (CurrentState) {
            case ThwompState.Ready:
                if (_player == null) break;
                if (_parent.Position.X - _triggerDistance < _player.Position.X
                    && _parent.Position.X + _triggerDistance > _player.Position.X) {
                    CurrentState = ThwompState.Falling;
                }
                break;
            
            case ThwompState.Falling:
                // 恢复实心判定
                _parent.SetCollisionMask(5);
                
                _speedY += _gravity;
                _parent.Position = new Vector2(_parent.Position.X, _parent.Position.Y + _speedY);
                _parent.ForceUpdateTransform();
                
                var collision = _parent.MoveAndCollide(new Vector2(0f, 1f), true, 0.01f);
                if (collision != null) {
                    do {
                        _parent.Position = _parent.Position with { Y = _parent.Position.Y - 1f };
                        _parent.ForceUpdateTransform();
                        collision = _parent.MoveAndCollide(Vector2.Zero, true, 0.01f);
                    } while (collision != null);
                    
                    // 瞬移到比原位更高的位置，记录更高位
                    if (_originY > _parent.Position.Y) {
                        _originY = _parent.Position.Y;
                    }
                    
                    // 砸砖判定
                    EmitSignal(SignalName.ThwompBlockHit);
                    
                    CurrentState = ThwompState.Grounded;
                    _speedY = 0f;
                    EmitSignal(SignalName.PlaySoundStun);
                    _parent.ResetPhysicsInterpolation();
                    
                    // 恢复无实心判定
                    _parent.SetCollisionMask(0);
                }
                break;
            
            case ThwompState.Grounded:
                _groundedTimer++;
                if (_groundedTimer < _groundedTime) break;
                _groundedTimer = 0;
                CurrentState = ThwompState.Reserve;
                break;
            
            case ThwompState.Reserve:
                if (_parent.Position.Y <= _originY) {
                    _parent.Position = _parent.Position with { Y = _originY };
                    _parent.ResetPhysicsInterpolation();
                    CurrentState = ThwompState.Ready;
                    break;
                }
                _parent.Position = _parent.Position with { Y = _parent.Position.Y - _reserveSpeed };
                break;
        }
    }
}
