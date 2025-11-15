using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Player;

public partial class ThwompPlayerInteraction : InteractionWithPlayer {
    [Signal]
    public delegate void ThwompTauntEventHandler();

    private bool _soundPlayEnd;
    private bool _animationFinished;

    public enum ThwompTauntState {
        Normal,
        Taunting,
    }
    private ThwompTauntState _currentState = ThwompTauntState.Normal;

    public void TryResetState() {
        if (!_animationFinished || !_soundPlayEnd) return;
        _animationFinished = false;
        _soundPlayEnd = false;
        _currentState = ThwompTauntState.Normal;
    }
    public override void PlayerHurtCheck(bool check) {
        if (!check || _currentState != ThwompTauntState.Normal) return;
        _currentState = ThwompTauntState.Taunting;
        EmitSignal(SignalName.ThwompTaunt);
    }
    public void OnSoundPlayEnd() {
        _soundPlayEnd = true;
        TryResetState();
    }
    public void OnAnimationFinished() {
        _animationFinished = true;
        TryResetState();
    }
}
