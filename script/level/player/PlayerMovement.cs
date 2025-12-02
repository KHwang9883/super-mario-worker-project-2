using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;
using SMWP.Level.Debug;
using SMWP.Level.Physics;
using SMWP.Level.Tool;

namespace SMWP.Level.Player;

public partial class PlayerMovement : Node {
    [Signal]
    public delegate void JumpStartedEventHandler();
    [Signal]
    public delegate void SwimStartedEventHandler();
    
    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private Area2D _aroundWaterArea2D = null!;

    // 关卡引力
    private float _gravity;
    
    // 运动参数先用 GM8 单位（px/f），计算 Velocity 的时候转换为 Godot 单位（px/s）
    private const float FramerateOrigin = 50f;
    public int Direction = 1;
    public float SpeedX;
    private float _walkingAcceleration = 0.1f;
    private float _runningAcceleration = 0.3f;
    private float _maxWalkingSpeed = 3f;
    private float _maxRunningSpeed = 8f;
    private float _lastPositionX;
    
    public float SpeedY;
    private float _lastSpeedY;
    private float _maxFallingSpeed = 13f;
    
    private float _waterHorizontalAcceleration = 0.05f;
    private float _waterMaxWalkingSpeed = 1f;
    private float _waterMaxRunningSpeed = 3f;
    private float _waterMaxFallingSpeed = 6f;
    
    public bool IsInWater;
    private bool _wasInWater;
    public bool IsOnWaterSurface = true;
    public bool IsAroundWater;

    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    public bool Fire;
    private bool _jump;
    public bool Jumped;
    public bool Crouched;
    private bool _wasCrouched;
    public bool Stuck;
    private bool _wasStuck;
    private int _jumpBoostTimer;

    public bool OnFallingPlatform;
    
    // TODO: 冰块状态
    public bool OnIce;
    
    // God Mode 玩家碰撞层掩码记录
    private uint _originPlayerLayer;
    private uint _originPlayerMask;

    private NodePath _areaBodyCollision = "AreaBodyCollisionSmall";
    private NodePath _outWaterDetect = "OutWaterDetectSmall";
    
    private Array<Node2D> _results = null!;
    private Array<Node2D> _resultsOutWater = null!;

    private CollisionPolygon2D _blocksPhysicsCollisionSmall = null!;
    private ShapeCast2D _areaBodyCollisionSmall = null!;
    private ShapeCast2D _outWaterDetectSmall = null!;
    private CollisionPolygon2D _blocksPhysicsCollisionSuper = null!;
    private ShapeCast2D _areaBodyCollisionSuper = null!;
    private ShapeCast2D _outWaterDetectSuper = null!;

    public override void _Ready() {
        // Checkpoint
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");
        if (checkpoints != null)
            foreach (var node in checkpoints) {
                if (node is not Checkpoint checkpoint) continue;
                if (LevelManager.CurrentCheckpointId != checkpoint.Id) continue;
                _player.Position = checkpoint.Position + Vector2.Up * 8f;
            }
        // 关卡重力设置
        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _gravity = levelConfig.Gravity / 5f;
        
        // _lastPositionX 初始化
        _lastPositionX = _player.Position.X;
        
        _originPlayerLayer = _player.GetCollisionLayer();
        _originPlayerMask = _player.GetCollisionMask();
        
        _blocksPhysicsCollisionSmall = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSmall");
        _areaBodyCollisionSmall = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSmall");
        _outWaterDetectSmall = _player.GetNode<ShapeCast2D>("OutWaterDetectSmall");
        _blocksPhysicsCollisionSuper = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSuper");
        _areaBodyCollisionSuper = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSuper");
        _outWaterDetectSuper = _player.GetNode<ShapeCast2D>("OutWaterDetectSuper");
    }

    public override void _PhysicsProcess(double delta) {
        // 输入处理
        _up = Input.IsActionPressed("move_up");
        _down = Input.IsActionPressed("move_down");
        _left = Input.IsActionPressed("move_left");
        _right = Input.IsActionPressed("move_right");
        Fire = Input.IsActionPressed("move_fire");
        _jump = Input.IsActionPressed("move_jump");

        // God Mode
        if (_playerMediator.playerGodMode.IsGodFly) {
            var direction = new Vector2();
            
            if (_right) direction.X += 1;
            if (_left) direction.X -= 1;
            if (_down) direction.Y += 1;
            if (_up) direction.Y -= 1;
            
            if (direction.Length() > 0) {
                direction = direction.Normalized();
            }
            _player.Position += direction * 8f;
            _player.SetCollisionLayer(0);
            _player.SetCollisionMask(0);
            return;
        }
        
        _player.SetCollisionLayer(_originPlayerLayer);
        _player.SetCollisionMask(_originPlayerMask);
        
        // x 速度
        float acceleration = IsInWater
            ? _waterHorizontalAcceleration
            : (Fire ? _runningAcceleration : _walkingAcceleration);

        float maxSpeed;

        if (IsInWater) {
            if (Fire) {
                maxSpeed = _waterMaxRunningSpeed;
            } else {
                maxSpeed = Mathf.Abs(SpeedX) > _waterMaxWalkingSpeed
                    ? _waterMaxRunningSpeed
                    : _waterMaxWalkingSpeed;
            }
        } else {
            if (Fire) {
                maxSpeed = _maxRunningSpeed;
            } else {
                maxSpeed = Mathf.Abs(SpeedX) > _maxWalkingSpeed
                    ? _maxRunningSpeed
                    : _maxWalkingSpeed;
            }
        }
        
        if (!Crouched && !Stuck) {
            if (_left) {
                SpeedX = Mathf.Clamp(SpeedX - acceleration, -maxSpeed, maxSpeed);
            }
            if (_right) {
                SpeedX = Mathf.Clamp(SpeedX + acceleration, -maxSpeed, maxSpeed);
            }
        }
        if (!_left && !_right || Crouched) {
            SpeedX /= IsInWater ? 1.03f : 1.05f;
        }
        if (SpeedX is > -0.04f and < 0.04f) {
            SpeedX = 0f;
        }
        
        // 方向记录
        Direction = SpeedX switch {
            < 0f => -1,
            > 0f => 1,
            _ => Direction,
        };

        // y 速度
        _lastSpeedY = SpeedY;
        
        // 落地或顶头
        if ((_player.IsOnFloor() || (_player.IsOnCeiling() && SpeedY < 0f))) {
            SpeedY = 0f;
        }
        
        // 大个子下蹲与起身
        if (_playerMediator.playerSuit.Suit != PlayerSuit.SuitEnum.Small) {
            if (_player.IsOnFloor() && _down && !Stuck) {
                Crouched = true;
            }
        }
        if (!_down) {
            Crouched = false;
        }
        
        // 起跳
        if (!_jump) {
            Jumped = false;
        }

        if (_jump && !Crouched && !Stuck && !_wasCrouched) {
            if (!IsInWater && _player.IsOnFloor()) {
                if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Powered
                    && _playerMediator.playerSuit.Powerup == PlayerSuit.PowerupEnum.Lui) {
                    SpeedY = -(9f + Mathf.Abs(SpeedX) / 5f);
                } else {
                    SpeedY = -(8f + Mathf.Abs(SpeedX) / 5f);
                }
                Jumped = true;
                EmitSignal(SignalName.JumpStarted);
            } else if (IsInWater && !Jumped) {
                SpeedY = IsOnWaterSurface
                    ? -(6f + Mathf.Abs(SpeedX) / 5f)
                    : -(4f + Mathf.Abs(SpeedX) / 10f);
                Jumped = true;
                EmitSignal(SignalName.SwimStarted);
            }
        }
        
        _wasCrouched = Crouched;

        // 空中跳跃按跳跃键有速度加成
        _jumpBoostTimer = Math.Clamp(_jumpBoostTimer + 1, 0, 2);

        if (_jump && SpeedY < 0f && !IsInWater) {
            if (_jumpBoostTimer > 1) {
                SpeedY -= 1.5f;
                _jumpBoostTimer = 0;
            }
        }

        // 最大下落速度
        if (!_player.IsOnFloor()) {
            float maxFallSpeed = IsInWater ? _waterMaxFallingSpeed : _maxFallingSpeed;

            if (SpeedY > maxFallSpeed) {
                SpeedY = maxFallSpeed;
            }
        }
        
        // 根据GM8版执行顺序在这里进行速度计算并 MoveAndSlide()
        _player.Velocity = new Vector2(SpeedX * FramerateOrigin, (SpeedY + _gravity) * FramerateOrigin);

        // 在 MoveAndSlide() 之前执行下蹲起立卡墙的挤出方法
        var collision = _player.MoveAndCollide(Vector2.Zero, true);
        if (collision != null) {
            Stuck = true;
        } else {
            Stuck = false;
        }
        
        // 空中卡天花板挤出
        if (Stuck && !_wasStuck) {
            // 空中从天花板挤出只执行一次（比如空中蹲起和小个子获得补给），因此用 _wasStuck 判断
            for (var i = 0; i <= 48; i++) {
                var originPosition = _player.Position;
                _player.Position = new Vector2(_player.Position.X, _player.Position.Y + i);
                var collisionInAir = _player.MoveAndCollide(new Vector2(0f, 0f), false);
                //GD.Print("向下移动 " + i + " 像素");
                if (collisionInAir == null) {
                    _player.Position = new Vector2(_player.Position.X, _player.Position.Y + i);
                    //GetTree().Paused = true;
                    Stuck = false;
                    break;
                }
                _player.Position = originPosition;
            }
        }

        _wasStuck = Stuck;
        
        if (Stuck) {
            // 蹲滑起立卡墙挤出
            SpeedX = 0f;
            SpeedY = 0f;
            _player.Position = new Vector2(_player.Position.X - 1f * Direction, _player.Position.Y);
        } else {
            // 正常运动
            _player.MoveAndSlide();
            
            // 掉落平台检测
            OnFallingPlatform = false;
            if (_player.IsOnFloor()) {
                var onPlatformTest = _player.MoveAndCollide(Vector2.Zero, true);
                var kinematicCollision2D = _player.MoveAndCollide(Vector2.Down, true);
                if (onPlatformTest == null && kinematicCollision2D != null) {
                    var collider = kinematicCollision2D.GetCollider();
                    if (IsInstanceValid(collider)) {
                        if (collider is ISteppable steppable) {
                            // 额外检测 y 速度的特性
                            if (_lastSpeedY > 0f /*_gravity*/) {
                                steppable.OnStepped();
                                OnFallingPlatform = true;
                            }
                        }
                    }
                }
            }
        }

        // 镜头越界处理（放在运动结束之后）
        var screen = ScreenUtils.GetScreenRect(this);
        if (_player.Position.X < screen.Position.X + 14f)
            _player.Position = new Vector2(Mathf.Max(_player.Position.X, _lastPositionX), _player.Position.Y);
        if (_player.Position.X > screen.End.X - 14f)
            _player.Position = new Vector2(Mathf.Min(_player.Position.X, _lastPositionX), _player.Position.Y);
        _lastPositionX = _player.Position.X;

        
        // 重叠物件检测
        try {
            _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_areaBodyCollision));
            _resultsOutWater =
                ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_outWaterDetect));
        } catch {
            GD.PrintErr("重叠物件检测失败");
            GD.Print("启用临时重力修正");
            SpeedY += (IsInWater ? 0.2f : 1.0f);
        }

        // 水中状态检测
        IsInWater = false;
        foreach (var result in _results) {
            if (result.IsInGroup("water")) {
                IsInWater = true;
            }
        }
        if (_wasInWater != IsInWater) {
            SpeedY = Mathf.Min(0f, SpeedY);
        }
        _wasInWater = IsInWater;

        IsOnWaterSurface = true;
        foreach (var resultOutWater in _resultsOutWater) {
            if (resultOutWater.IsInGroup("water")) {
                IsOnWaterSurface = false;
            }
        }

        // Debug
        //GD.Print(ShapeQueryResult.ShapeQuery(this, GetNode<ShapeCast2D>("AreaBodyCollision")));

        // 重力
        SpeedY += (IsInWater ? 0.2f : 1.0f);

        // GM8版注释：尝试性修复非整格实心穿墙
        // 为保持精确性，故各自方向速度为零分别进行一次取整，但是该做法会导致楼梯地形贴墙原地起跳边缘卡脚，因此禁用

        // 第 146 号 BGM 靠近流体检测
        IsAroundWater = _aroundWaterArea2D.GetOverlappingAreas().Count > 0;
        
        // 不同状态的碰撞箱切换
        Callable.From(() => {
            if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Small || Crouched) {
                _areaBodyCollision = "AreaBodyCollisionSmall";
                _outWaterDetect = "OutWaterDetectSmall";
                _blocksPhysicsCollisionSmall.Disabled = false;
                _areaBodyCollisionSmall.Enabled = true;
                _outWaterDetectSmall.Enabled = true;
                _blocksPhysicsCollisionSuper.Disabled = true;
                _areaBodyCollisionSuper.Enabled = false;
                _outWaterDetectSuper.Enabled = false;
            } else {
                _areaBodyCollision = "AreaBodyCollisionSuper";
                _outWaterDetect = "OutWaterDetectSuper";
                _blocksPhysicsCollisionSmall.Disabled = true;
                _areaBodyCollisionSmall.Enabled = false;
                _outWaterDetectSmall.Enabled = false;
                _blocksPhysicsCollisionSuper.Disabled = false;
                _areaBodyCollisionSuper.Enabled = true;
                _outWaterDetectSuper.Enabled = true;
            }
        }).CallDeferred();

        // For debug use
        _areaBodyCollisionSmall.Visible = _areaBodyCollisionSmall.Enabled;
        _areaBodyCollisionSuper.Visible = _areaBodyCollisionSuper.Enabled;
    }
    public Array<Node2D> GetShapeQueryResults() {
        return _results;
    }
    public void OnPlayerStomp(float stompSpeedY = 8f) {
        SpeedY = stompSpeedY;
    }
}
