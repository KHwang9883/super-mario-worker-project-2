using Godot;
using System;
using SMWP.Level.Tool;

public partial class FallingStar : Area2D {
    private Sprite2D? _sprite;
    private bool _fadeOut;
    private FallingStarsController? _fallingStarsController;
    private Vector2 _scrPosStart;
    private Vector2 _scrPosEnd;
    private Area2D? _forceReturnToPoolArea2D;
    private RandomNumberGenerator _rng = new();
    private float _speed;
    private float _angle;
    private float _angleSpeed;
    
    public override void _Ready() {
        _sprite ??= GetNode<Sprite2D>("Sprite2D");
        _forceReturnToPoolArea2D ??= GetNode<Area2D>("ForceReturnToPool");
        _fallingStarsController =
            (FallingStarsController)GetTree().GetFirstNodeInGroup("falling_star_controller");
        Reset();
    }
    public void Reset() {
        Visible = true;
        _fadeOut = false;
        Modulate = Modulate with { A = 1f };
        _angle = 180 + 70 + _rng.RandfRange(0, 30) - _rng.RandfRange(0, 30);
        _speed = 4f + _rng.RandfRange(0, 3);
        _angleSpeed = 2 + _rng.RandiRange(0, 3);
        if (_fallingStarsController is { FallingStarsLevel: 3 }) {
            _speed *= 1.2f;
        }
        var screen = ScreenUtils.GetScreenRect(this);
        _scrPosStart = screen.Position;
        _scrPosEnd = screen.Position;
        ResetPhysicsInterpolation();
    }
    public override void _PhysicsProcess(double delta) {
        // Movement
        Vector2 direction = new Vector2(
            Mathf.Cos(Mathf.DegToRad(_angle)),
            -Mathf.Sin(Mathf.DegToRad(_angle))
        );
        
        Vector2 velocity = direction * _speed;
        
        Position += velocity;
        
        if (_sprite != null) _sprite.RotationDegrees += _angleSpeed;
        
        // 玩家大距离瞬移则雨滴跟着瞬移
        _scrPosEnd = ScreenUtils.GetScreenRect(this).Position;
        if (Mathf.Abs(_scrPosEnd.X - _scrPosStart.X) > 128f) {
            Position += new Vector2(_scrPosEnd.X - _scrPosStart.X, 0f);
            ResetPhysicsInterpolation();
        }
        if (Mathf.Abs(_scrPosEnd.Y - _scrPosStart.Y) > 128f) {
            Position += new Vector2(0f, _scrPosEnd.Y - _scrPosStart.Y);
            ResetPhysicsInterpolation();
        }
        _scrPosStart = _scrPosEnd;
        
        // 碰到水域
        if (GetOverlappingAreas().Count > 0) {
            _fadeOut = true;
        }
        if (_fadeOut) {
            Modulate = Modulate with { A = Mathf.MoveToward(Modulate.A, 0, 0.2f) };
        }
        if (Modulate.A == 0f) {
            ReturnToPool();
        }
        // 意外直接较深进入水域，强制回收
        if (_forceReturnToPoolArea2D != null
            &&_forceReturnToPoolArea2D.GetOverlappingAreas().Count > 0) {
            ReturnToPool();
        }
        
        // 离开屏幕
        if (Position.Y > ScreenUtils.GetScreenRect(this).End.Y + 64f) {
            ReturnToPool();
        }
    }
    public void ReturnToPool() {
        AddToGroup("falling_star_pool");
        Visible = false;
    }
}
