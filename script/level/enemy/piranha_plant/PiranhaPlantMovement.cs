using Godot;
using System;

public partial class PiranhaPlantMovement : Node {
    [ExportGroup("Movement")]
    [Export] private int _bitingTime = 100;
    [Export] private int _retractingTime = 50;
    [Export] private float _shyRangeX = 64f;
    [Export] private int _shyDuration = 150;
    [Export] private int _risingTime = 50;
    [Export] private float _angle;

    [ExportGroup("FireOption")]
    [Export] private bool _fire;
    
    private enum MoveState { Biting, Retracting, Waiting, Rising }
    private MoveState _currentState = MoveState.Biting;

    private int _stateTimer;
    private Node2D? _parent;
    private Node2D? _player;
    private RandomNumberGenerator _rng = new();
    
    private Vector2 MovementDirection {
        get {
            float radians = Mathf.DegToRad(_angle);
            return new Vector2(Mathf.Sin(radians), -Mathf.Cos(radians));
        }
    }

    public override void _Ready() {
        _parent ??= GetParent<Node2D>();
        _player ??= (Node2D)GetTree().GetFirstNodeInGroup("player");
        _stateTimer = -_rng.RandiRange(0, 100);
        
        if (_parent != null) {
            _parent.RotationDegrees = _angle;
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;

        _stateTimer++;

        switch (_currentState) {
            case MoveState.Biting when _stateTimer >= _bitingTime:
                TransitionState(MoveState.Retracting, 0);
                break;
            
            case MoveState.Retracting:
                _parent.Position -= MovementDirection;
                if (_stateTimer >= _retractingTime) 
                    TransitionState(MoveState.Waiting, 0);
                break;
            
            case MoveState.Waiting when _stateTimer >= _shyDuration:
                if (_player != null && Mathf.Abs(_player.Position.X - _parent.Position.X) > _shyRangeX)
                    TransitionState(MoveState.Rising, 0);
                break;
            
            case MoveState.Rising:
                _parent.Position += MovementDirection;
                if (_stateTimer >= _risingTime)
                    TransitionState(MoveState.Biting, 0);
                break;
        }
    }
    private void TransitionState(MoveState newState, int timerStart = 0) {
        _currentState = newState;
        _stateTimer = timerStart;
    }
}