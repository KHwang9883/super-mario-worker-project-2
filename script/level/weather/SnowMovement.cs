using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Util;

public partial class SnowMovement : Node {
    public enum SnowStateEnum { Falling, Piece, Free }
    private SnowStateEnum _state = SnowStateEnum.Falling;
    private bool _fadeOut;
    private Sprite2D? _sprite;
    private AnimatedSprite2D? _animation;
    private float _speed;
    private float _angleSpeed;
    //private float _alphaRate = 50f;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Area2D? _parent;
    private SnowyController? _snowyController;
    private WindyController? _windyController;
    
    private Vector2 _scrPosStart;
    private Vector2 _scrPosEnd;
    
    public int WindyLevel { get; set; }
    public int SnowyLevel { get; set; }

    public override void _Ready() {
        _parent ??= GetParent<Area2D>();
        _snowyController = (SnowyController)GetTree().GetFirstNodeInGroup("snowy_controller");
        _windyController = (WindyController)GetTree().GetFirstNodeInGroup("windy_controller");
        _sprite = GetNode<Sprite2D>("../Sprite2D");
        _animation = GetNode<AnimatedSprite2D>("../AnimatedSprite2D");
        Reset();
    }
    public void Reset() {
        if (_parent == null || _snowyController == null || _windyController == null
            || _sprite == null || _animation == null) return;

        _sprite.Visible = true;
        _animation.Visible = false;
        _state = SnowStateEnum.Falling;
        _parent.SetCollisionMask(69);
        _fadeOut = false;
        _parent.Modulate = _parent.Modulate with { A = 1f };
        WindyLevel = _windyController.WindyLevel;
        _parent.RotationDegrees = 180f - 70f + _rng.RandfRange(0, 10) - _rng.RandfRange(0, 10);
        _speed = 4f + _rng.RandfRange(0f, 3f);
        _angleSpeed = 2f + _rng.RandiRange(0, 3);
        
        // 大雪雪速度更大
        SnowyLevel = _snowyController.SnowyLevel;
        if (SnowyLevel == 5) {
            _speed *= 1.6f;
        }
        
        /*if (_rng.RandfRange(0f, 99f) < _alphaRate) {
            // 透明雪不参与墙体碰撞，只与水域碰撞
            _parent.Modulate = _parent.Modulate with { A = 0.2f };
            _parent.SetCollisionMask(64);
        }*/

        var screen = ScreenUtils.GetScreenRect(this);
        _scrPosStart = screen.Position;
        _scrPosEnd = screen.Position;
        _parent.ResetPhysicsInterpolation();
    }
    public override void _PhysicsProcess(double delta) {
        if (_parent == null || _sprite == null) return;
        if (_state == SnowStateEnum.Piece) {
            if (_animation != null && !_animation.IsPlaying()) {
                _animation.Visible = true;
                _animation.Play();
            }
            return;
        }
        
        // Movement
        Vector2 direction = new Vector2(
            Mathf.Cos(Mathf.DegToRad(_parent.RotationDegrees)),
            Mathf.Sin(Mathf.DegToRad(_parent.RotationDegrees))
        );
        
        Vector2 velocity = direction * _speed;

        velocity *= Mathf.Max(1f, WindyLevel * 1.2f);
        
        _parent.Position += velocity;
        
        _sprite.RotationDegrees += _angleSpeed;
        
        // 玩家大距离瞬移则雪跟着瞬移
        _scrPosEnd = ScreenUtils.GetScreenRect(this).Position;
        if (Mathf.Abs(_scrPosEnd.X - _scrPosStart.X) > 128f) {
            _parent.Position += new Vector2(_scrPosEnd.X - _scrPosStart.X, 0f);
            _parent.ResetPhysicsInterpolation();
        }
        if (Mathf.Abs(_scrPosEnd.Y - _scrPosStart.Y) > 128f) {
            _parent.Position += new Vector2(0f, _scrPosEnd.Y - _scrPosStart.Y);
            _parent.ResetPhysicsInterpolation();
        }
        _scrPosStart = _scrPosEnd;

        if (_parent.Position.Y > ScreenUtils.GetScreenRect(this).End.Y + 128f)
            ReturnToPool();
        
        // 因为 Godot Area2D 重叠检测 2 物理帧延迟，主要采用信号来触发进行闲置状态，
        // 防止恢复的时候仍然被判定与墙体重叠，此处是考虑一开始就生成在墙体或水域内的情形
        if (_parent.GetOverlappingAreas().Count > 0) ReturnToPool();
        if (_parent.GetOverlappingBodies().Count > 0) SetFadeOut();
        
        // 碰到水域
        if (GetNode<Area2D>("../WaterDetect").GetOverlappingAreas().Count > 0) {
            _fadeOut = true;
        }
        if (_fadeOut) {
            _parent.Modulate = _parent.Modulate with { A = Mathf.MoveToward(_parent.Modulate.A, 0, 0.2f) };
        }
        if (_parent.Modulate.A == 0f) {
            ReturnToPool();
        }
    }
    public void ReturnToPool() {
        _state = SnowStateEnum.Free;
        _parent?.SetCollisionMask(0);
        _parent?.AddToGroup("snow_pool");
        _sprite?.SetVisible(false);
        _animation?.SetVisible(false);
    }
    public void SetFadeOut() {
        _state = SnowStateEnum.Piece;
        _sprite?.SetVisible(false);
    }
}