using Godot;
using System;

namespace SMWP.Level.Tool;

[GlobalClass]
public partial class SmwpFrameTimer : Node {
    [Signal]
    public delegate void TimeoutEventHandler();
    
    [Export] public int Frame = 5;
    [Export] public bool OneShot;
    [Export] public bool AutoStart;
    [Export] public bool AutoStop;
    public int Timer;
    private bool _oneShotTriggered;
    private bool _startCount;

    public override void _Ready() {
        _startCount = AutoStart;
    }
    public override void _PhysicsProcess(double delta) {
        if (_oneShotTriggered || !_startCount) return;
        Timer++;
        if (Timer < Frame) return;
        EmitSignal(SignalName.Timeout);
        Timer = 0;
        if (OneShot) {
            _oneShotTriggered = true;
        }
        if (AutoStop) {
            _startCount = false;
        }
    }
    public bool IsStopped() {
        return !_startCount;
    }
    public void Start() {
        _startCount = true;
    }
    public void Stop() {
        _startCount = false;
    }
}
