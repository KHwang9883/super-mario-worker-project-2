using Godot;
using System;

public partial class PlatformFall : AnimatableBody2D, ISteppable {
    [Signal]
    public delegate void SteppedEventHandler();
    
    [Export] private float _gravity = 1f;
    [Export] private int _gravityTime = 10;
    private bool _fall;
    private bool _isSteppedOn;
    private float _speedY = 1f;
    private int _timer;

    public override void _PhysicsProcess(double delta) {
        if (!_fall) return;

        _timer++;
        if (_timer > _gravityTime) {
            _speedY += 1f;
            _timer = 0;
        }
        
        Position += new Vector2(0f, _speedY);
        
        // Todo: 玩家踩到掉落平台后，无视其他平台
    }
    public void OnStepped() {
        EmitSignal(SignalName.Stepped);
        _fall = true;
        _isSteppedOn = true;
    }
}
