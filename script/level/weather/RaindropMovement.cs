using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Tool;

public partial class RaindropMovement : Node {
    // 变量声明
    private float _speed;
    private float _alphaRate = 50f;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Area2D? _parent;
    private RainyController? _rainyController;
    private WindyController? _windyController;
    
    private Vector2 _scrPosStart;
    private Vector2 _scrPosEnd;
    
    public int WindyLevel { get; set; }
    public int RainyLevel { get; set; }

    public override void _Ready() {
        _parent ??= GetParent<Area2D>();
        _rainyController = (RainyController)GetTree().GetFirstNodeInGroup("rainy_controller");
        _windyController = (WindyController)GetTree().GetFirstNodeInGroup("windy_controller");
        Reset();
    }
    public void Reset() {
        if (_parent == null || _rainyController == null || _windyController == null) return;
        
        _parent.SetCollisionMask(69);
        _parent.Modulate = _parent.Modulate with { A = 1f };
        WindyLevel = _windyController.WindyLevel;
        _parent.RotationDegrees = 180f - 70f + WindyLevel * 12.0f;
        _speed = 8f + _rng.RandfRange(0f, 6f);
        
        // 大雨雨滴速度更大
        RainyLevel = _rainyController.RainyLevel;
        if (RainyLevel == 5) {
            _speed *= 1.6f;
        }
        
        if (_rng.RandfRange(0f, 99f) < _alphaRate) {
            // 透明雨滴不参与墙体碰撞，只与碰撞
            _parent.Modulate = _parent.Modulate with { A = 0.2f };
            _parent.SetCollisionMask(64);
        }

        var screen = ScreenUtils.GetScreenRect(this);
        _scrPosStart = screen.Position;
        _scrPosEnd = screen.Position;
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null) return;
        
        Vector2 direction = new Vector2(
            Mathf.Cos(Mathf.DegToRad(_parent.RotationDegrees)),
            Mathf.Sin(Mathf.DegToRad(_parent.RotationDegrees))
        );
        
        Vector2 velocity = direction * _speed;

        velocity *= Mathf.Max(1f, WindyLevel * 1.2f);
        
        _parent.Position += velocity;
        
        _scrPosEnd = ScreenUtils.GetScreenRect(this).Position;
        
        // 玩家大距离瞬移则雨滴跟着瞬移
        if (Mathf.Abs(_scrPosEnd.X - _scrPosStart.X) > 128f) {
            _parent.Position += new Vector2(_scrPosEnd.X - _scrPosStart.X, 0f);
        }
        if (Mathf.Abs(_scrPosEnd.Y - _scrPosStart.Y) > 128f) {
            _parent.Position += new Vector2(0f, _scrPosEnd.Y - _scrPosStart.Y);
        }
        
        _scrPosStart = _scrPosEnd;

        if (_parent.Position.Y > ScreenUtils.GetScreenRect(this).End.Y + 128f)
            ReturnToPool();

        if (_parent.GetOverlappingAreas().Count > 0) ReturnToPool();
    }
    public void ReturnToPool() {
        _parent?.AddToGroup("raindrop_pool");
        _parent?.SetVisible(false);
    }
}