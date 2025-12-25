using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;
using SMWP.Level.Debug;
using SMWP.Level.Physics;
using SMWP.Util;

namespace SMWP.Level.Player;

public partial class PlayerMovement : Node {
    [Signal]
    public delegate void JumpStartedEventHandler();
    [Signal]
    public delegate void SwimStartedEventHandler();
    [Signal]
    public delegate void PipeInEventHandler();
    [Signal]
    public delegate void SetPipeOutInvincibleEventHandler();
    [Signal]
    public delegate void ForceScrollDeathEventHandler();
    [Signal]
    public delegate void PlaySoundPowerDownEventHandler();

    [Export] private PlayerMediator _playerMediator = null!;
    [Export] private CharacterBody2D _player = null!;
    [Export] private Area2D _onIceArea2D = null!;
    [Export] private Area2D _aroundWaterArea2D = null!;
    
    private LevelConfig? _levelConfig;
    
    // 水管传送状态
    public bool IsInPipeTransport;
    private bool _wasInPipe;
    private bool _wasPipeOut;
    public int PipeTransportId;
    private int _pipeInTransportTimer;

    public enum PipeTransportStatusEnum {
        In,
        Out,
    }
    public PipeTransportStatusEnum PipeTransportStatus;

    public enum PipeTransportDirection {
        Up,
        Down,
        Left,
        Right,
    }
    public PipeTransportDirection PipeTransportDir;

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
    public float LastPositionX;

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

    // 入力のprocess
    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    
    public bool Fire;
    
    private bool _jump;
    public bool Jumped;
    private int _jumpBoostTimer;
    
    public bool Crouched;
    private bool _wasCrouched;
    
    public bool Stuck;
    public bool StuckSwitch;
    private bool _wasStuck;

    public bool OnVerticalPlatform;

    // 在冰块上状态
    public bool IsOnIce;

    // God Mode 玩家碰撞层掩码记录
    private uint _originPlayerLayer;
    private uint _originPlayerMask;

    private NodePath _areaBodyCollision = "AreaBodyCollisionSmall";
    private NodePath _outWaterDetect = "OutWaterDetectSmall";

    private Array<Node2D>? _results;
    private Array<Node2D> _resultsOutWater = null!;

    private CollisionPolygon2D _blocksPhysicsCollisionSmall = null!;
    private ShapeCast2D _areaBodyCollisionSmall = null!;
    private ShapeCast2D _outWaterDetectSmall = null!;
    private CollisionPolygon2D _blocksPhysicsCollisionSuper = null!;
    private ShapeCast2D _areaBodyCollisionSuper = null!;
    private ShapeCast2D _outWaterDetectSuper = null!;

    private LevelCamera? _levelCamera;
    private bool _levelStartAutoScrollDetect;
    private bool _autoScrollCheckpointDetect;
    private bool _autoScrollCheckpointDetected;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        
        // Checkpoint
        var checkpoints = GetTree().GetNodesInGroup("checkpoint");
        if (checkpoints != null) {
            foreach (var node in checkpoints) {
                if (node is not Checkpoint checkpoint) continue;
                if (GameManager.CurrentCheckpointId != checkpoint.Id) continue;
                SetPositionToCheckpoint(checkpoint);

                _autoScrollCheckpointDetect = true;

                // 设置 CP 触发时记录的流体高度见 LevelConfig
            }
        }
        
        // 关卡重力设置
        var levelConfig = LevelConfigAccess.GetLevelConfig(this);
        _gravity = levelConfig.Gravity / 5f;

        // _lastPositionX 初始化
        LastPositionX = _player.Position.X;

        _originPlayerLayer = _player.GetCollisionLayer();
        _originPlayerMask = _player.GetCollisionMask();

        _blocksPhysicsCollisionSmall = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSmall");
        _areaBodyCollisionSmall = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSmall");
        _outWaterDetectSmall = _player.GetNode<ShapeCast2D>("OutWaterDetectSmall");
        _blocksPhysicsCollisionSuper = _player.GetNode<CollisionPolygon2D>("BlocksPhysicsCollisionSuper");
        _areaBodyCollisionSuper = _player.GetNode<ShapeCast2D>("AreaBodyCollisionSuper");
        _outWaterDetectSuper = _player.GetNode<ShapeCast2D>("OutWaterDetectSuper");

        // 镜头控制元件检测
        
        // 顺序必须比较晚，因为 SceneControl 可能因为读取顺序后进入场景树
        // 因此与 ViewControl 的连接检测放在帧末执行
        // 因而 Player 的检测必须更晚，故用 SmwlLevel 的 LevelLoaded 信号作为检测的时间点
        
        var smwlLevel = (SmwlLevel)GetTree().GetFirstNodeInGroup("smwl_level");
        if (smwlLevel != null) {
            //GD.Print("PlayerMovement: SmwlLevel got.");
            smwlLevel.LevelLoaded += ViewControlDetect;
        }
        // 用于测试关卡测试
        else {
            Callable.From(ViewControlDetect).CallDeferred();
        }
        
        // 摄像机
        _levelCamera ??= (LevelCamera)GetTree().GetFirstNodeInGroup("camera");
    }

    public override void _PhysicsProcess(double delta) {
        // 输入处理
        _up = Input.IsActionPressed("move_up");
        _down = Input.IsActionPressed("move_down");
        _left = Input.IsActionPressed("move_left");
        _right = Input.IsActionPressed("move_right");
        Fire = Input.IsActionPressed("move_fire");
        _jump = Input.IsActionPressed("move_jump");
        
        // 自动滚屏检测
        AutoScrollDetect();
        
        // 库巴滚屏检测
        KoopaScrollDetect();
        
        // 在水管传送状态下不进行其他运动处理
        PipeEntryDetect();

        PipeTransport();

        if (IsInPipeTransport) return;

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

        if (!StuckSwitch) {
            // x 速度
            var runningAcceleration = !IsOnIce ? _runningAcceleration : (_runningAcceleration - 0.2f);
            var walkingAcceleration = !IsOnIce ? _walkingAcceleration : (_walkingAcceleration - 0.05f);
            var waterHorizontalAcceleration = !IsOnIce ? _waterHorizontalAcceleration : 0f;

            var acceleration = IsInWater
                ? waterHorizontalAcceleration
                : (Fire ? runningAcceleration : walkingAcceleration);

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
            if (SpeedX is > -0.04f and < 0.04f
                || (IsInWater && IsOnIce)) {
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

            if (_jump && !Crouched && !Stuck && !_wasCrouched && !_wasInPipe) {
                if (!IsInWater && _player.IsOnFloor()) {
                    Jump();
                } else if (IsInWater && !Jumped) {
                    Swim();
                }
            }

            _wasInPipe = IsInPipeTransport;

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
        }

        // 在 MoveAndSlide() 之前执行下蹲起立卡墙的挤出方法
        var collision = _player.MoveAndCollide(Vector2.Zero, true);
        if (collision != null) {
            Stuck = true;
            
            // 开关砖卡墙
            foreach (var node in GetShapeQueryResults()) {
                if (node is not DottedLineBlock dottedLineBlock) continue;
                if (!dottedLineBlock.JustSolid) continue;
                StuckSwitch = true;
                break;
            }
        } else {
            Stuck = false;
            StuckSwitch = false;
        }

        // 空中卡天花板挤出
        if (Stuck && !_wasStuck) {
            // 空中从天花板挤出只执行一次（比如空中蹲起和小个子获得补给），因此用 _wasStuck 判断
            for (var i = 0; i <= 48; i++) {
                var originPosition = _player.Position;
                _player.Position = new Vector2(_player.Position.X, _player.Position.Y + i);
                var collisionInAir = _player.MoveAndCollide(Vector2.Zero, false);
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

        if (_levelConfig == null) {
            GD.PushError("Player: Push out from ceiling: LevelConfig is null!");
            return;
        }
        
        if (!StuckSwitch) {
            if (Stuck) {
                if (_levelConfig.ModifiedMovement) {
                    // 蹲滑起立卡墙挤出
                    SpeedX = 0f;
                    SpeedY = 0f;
                    _player.Position += new Vector2(-1f * Direction, 0f);
                } else {
                    // ModifiedMovement 关闭 / Classic Ver. 的瞬间挤出
                    if (_jump && Math.Abs(SpeedX) < 0.5f) {
                        while (_player.MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
                            // 绿果状态挤出（对引力有要求、对 x 速度有进一步要求）
                            if (_gravity < 10f
                                && _playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Powered
                                && _playerMediator.playerSuit.Powerup == PlayerSuit.PowerupEnum.Lui
                                && Math.Abs(SpeedX) < 0.2f) {
                                _player.Position += Vector2.Up;
                            }
                            // 向下挤出
                            else {
                                _player.Position += Vector2.Down;
                            }
                        }
                    } else {
                        // 横向挤出
                        // 反身 / 反向挤出（对 x 速度有要求）
                        if (Math.Abs(SpeedX) < 0.5f) {
                            if (_left) {
                                Direction = -1;
                            }
                            if (_right) {
                                Direction = 1;
                            }
                        }
                        while (_player.MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
                            _player.Position += new Vector2(-1f * Direction, 0f);
                        }
                    }
                    _player.ResetPhysicsInterpolation();
                }
            } else {
                // 正常运动
                _player.MoveAndSlide();

                // 掉落平台检测
                OnVerticalPlatform = false;
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
                                    OnVerticalPlatform = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // 镜头越界处理（放在运动结束之后）
        var screen = ScreenUtils.GetScreenRect(this);
        if (_player.Position.X < screen.Position.X + 14f)
            _player.Position = new Vector2(Mathf.Max(_player.Position.X, LastPositionX), _player.Position.Y);
        if (_player.Position.X > screen.End.X - 14f)
            _player.Position = new Vector2(Mathf.Min(_player.Position.X, LastPositionX), _player.Position.Y);
        
        // 自由滚屏下不会强制挤出玩家；强制滚屏下会挤出玩家
        if (_levelCamera == null) {
            GD.PushError($"{this}: LevelCamera is null!");
        } else {
            if (_levelCamera.CameraMode != LevelCamera.CameraModeEnum.FollowPlayer) {
                // 玩家不与墙体重合时才进行强制挤出
                if (_player.MoveAndCollide(new Vector2(_levelCamera.DeltaPosition.X, 0f), true, 0.02f) == null) {
                    var forceScrollPush =
                        Mathf.Abs(_levelCamera.DeltaPosition.X) + Mathf.Abs(_player.Position.X - LastPositionX);
                
                    if (_player.Position.X < screen.Position.X + 14f)
                        _player.Position += Vector2.Right * forceScrollPush;
                    if (_player.Position.X > screen.End.X - 14f)
                        _player.Position += Vector2.Left * forceScrollPush;
                }

                switch (_levelCamera.CameraMode) {
                    case LevelCamera.CameraModeEnum.AutoScroll:
                        // 在自动滚屏下在左或右一侧界外则死亡
                        if (_player.Position.X < screen.Position.X - 14f || _player.Position.X > screen.End.X + 14f) {
                            EmitSignal(SignalName.ForceScrollDeath);
                        }
                        break;
                    
                    case LevelCamera.CameraModeEnum.Koopa:
                        // 在库巴滚屏下在左或右一侧界边缘则死亡
                        if (_player.MoveAndCollide(new Vector2(_levelCamera.DeltaPosition.X, 0f), true, 0.02f) != null) {
                            if (_player.Position.X < screen.Position.X + 14f || _player.Position.X > screen.End.X - 14f) {
                                EmitSignal(SignalName.ForceScrollDeath);
                            }
                        }
                        break;
                }
            }
        }
        
        LastPositionX = _player.Position.X;

        // 重叠物件检测
        OverlapDetect();

        // 水中状态检测
        InWaterDetect();
        
        // 入水瞬间限制 y 速度
        if (_wasInWater != IsInWater) {
            SpeedY = Mathf.Min(0f, SpeedY);
            // 并且解除 IsOnIce 状态
            IsOnIce = false;
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

        // 冰块检测
        OnIceDetect();
        //GD.Print(IsOnIce);
        
        // 第 146 号 BGM 靠近流体检测
        IsAroundWater = _aroundWaterArea2D.GetOverlappingAreas().Count > 0;

        // 不同状态的碰撞箱切换
        UpdateHitBox();
    }

    // 获取重叠物件检测结果，供其他组件使用
    public Array<Node2D> GetShapeQueryResults() {
        if (_results != null) return _results;
        _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_areaBodyCollision));
        return _results;
    }

    public void Jump() {
        if (_playerMediator.playerSuit.Suit == PlayerSuit.SuitEnum.Powered
            && _playerMediator.playerSuit.Powerup == PlayerSuit.PowerupEnum.Lui) {
            SpeedY = -(9f + Mathf.Abs(SpeedX) / 5f);
        } else {
            SpeedY = -(8f + Mathf.Abs(SpeedX) / 5f);
        }
        Jumped = true;
        EmitSignal(SignalName.JumpStarted);
    }

    public void Swim() {
        SpeedY = IsOnWaterSurface
            ? -(6f + Mathf.Abs(SpeedX) / 5f)
            : -(4f + Mathf.Abs(SpeedX) / 10f);
        Jumped = true;
        EmitSignal(SignalName.SwimStarted);
    }

    // 库巴踩踏设置 x 速度
    public void OnStompSetSpeedX(float stompSpeedX) {
        SpeedX = stompSpeedX * Direction;
        //GD.Print($"Current player SpeedX: {SpeedX}");
    }
    
    public void UpdateHitBox() {
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
    
    // 玩家踩踏处理
    public void OnPlayerStomp(float stompSpeedY = 8f) {
        SpeedY = stompSpeedY;
    }

    public void InWaterDetect() {
        IsInWater = false;
        if (_results == null) return;
        foreach (var result in _results) {
            if (!result.IsInGroup("water")) continue;
            IsInWater = true;
            break;
        }
        //GD.Print(IsInWater);
    }
    
    // 重叠检测
    public void OverlapDetect() {
        try {
            _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_areaBodyCollision));
            _resultsOutWater =
                ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_outWaterDetect));
        } catch {
            GD.PrintErr("重叠物件检测失败");
            GD.Print("启用临时重力修正");
            SpeedY += (IsInWater ? 0.2f : 1.0f);
        }
    }
    
    // 中途点位置设置
    public void SetPositionToCheckpoint(Checkpoint checkpoint) {
        Callable.From(() => {
            _player.Position = checkpoint.Position + Vector2.Up * 12f;
            _player.ResetPhysicsInterpolation();
            LastPositionX = _player.Position.X;
        }).CallDeferred();
    }
    
    // 水管传送处理
    public void PipeEntryDetect() {
        if (IsInPipeTransport) return;

        foreach (var result in GetShapeQueryResults()) {
            Node? pipeEntryNode = null;
            if (result == null) continue;

            if (result.HasMeta("PipeEntry")) {
                pipeEntryNode = (Node2D)result.GetMeta("PipeEntry");
            }
            if (pipeEntryNode is not PassageIn pipeEntryInfo) continue;
            PipeTransportId = pipeEntryInfo.PassageId;
            switch (pipeEntryInfo.Direction) {
                case PassageIn.PassageDirection.Up:
                    if (_up) {
                        PipeInPlayerSet(PipeTransportDirection.Up);
                        _player.Position = new Vector2(pipeEntryInfo.Position.X, _player.Position.Y);
                    }
                    break;
                case PassageIn.PassageDirection.Down:
                    if (_down) {
                        PipeInPlayerSet(PipeTransportDirection.Down);
                        _player.Position = new Vector2(pipeEntryInfo.Position.X, _player.Position.Y);
                    }
                    break;
                case PassageIn.PassageDirection.Left:
                    if (_left) {
                        PipeInPlayerSet(PipeTransportDirection.Left);
                        _player.Position = new Vector2(_player.Position.X, pipeEntryInfo.Position.Y + 32f - 12f);
                    }
                    break;
                case PassageIn.PassageDirection.Right:
                    if (_right) {
                        PipeInPlayerSet(PipeTransportDirection.Right);
                        _player.Position = new Vector2(_player.Position.X, pipeEntryInfo.Position.Y + 32f - 12f);
                    }
                    break;
            }
        }
    }

    public void PipeInPlayerSet(PipeTransportDirection direction) {
        // 进入水管传送状态设置
        PipeTransportStatus = PipeTransportStatusEnum.In;
        PipeTransportDir = direction;
        IsInPipeTransport = true;
        _pipeInTransportTimer = 0;
        SpeedX = 0f;
        SpeedY = 0f;
        _player.ResetPhysicsInterpolation();
        EmitSignal(SignalName.PipeIn);
        EmitSignal(SignalName.PlaySoundPowerDown);
    }

    public void PipeTransport() {
        if (!IsInPipeTransport) return;
        
        // 进行水中检测
        OverlapDetect();
        InWaterDetect();
        
        // 水管传送状态移动处理
        switch (PipeTransportStatus) {
            // 进入水管
            case PipeTransportStatusEnum.In:
                _pipeInTransportTimer++;
                if (_pipeInTransportTimer <= 33) {
                    switch (PipeTransportDir) {
                        case PipeTransportDirection.Up:
                            _player.Position -= new Vector2(0f, 0.7f);
                            break;
                        case PipeTransportDirection.Down:
                            _player.Position += new Vector2(0f, 0.7f);
                            break;
                        case PipeTransportDirection.Left:
                            _player.Position -= new Vector2(0.7f, 0f);
                            break;
                        case PipeTransportDirection.Right:
                            _player.Position += new Vector2(0.7f, 0f);
                            break;
                    }
                } else {
                    _pipeInTransportTimer = 0;

                    // 传送到对应位置
                    var passageOuts = GetTree().GetNodesInGroup("passage_out");
                    foreach (var node in passageOuts) {
                        if (node is not PassageOut passageOut) continue;
                        if (passageOut.PassageId != PipeTransportId) continue;

                        // 传送位置微调
                        _player.Position = passageOut.Direction switch {
                            PassageIn.PassageDirection.Up => passageOut.Position + new Vector2(0f, 32f - 12f),
                            PassageIn.PassageDirection.Down => passageOut.Position + new Vector2(0f, 64f - 12f),
                            PassageIn.PassageDirection.Left => passageOut.Position + new Vector2(-16f, 32f - 12f),
                            PassageIn.PassageDirection.Right => passageOut.Position + new Vector2(16f, 32f - 12f),
                            _ => _player.Position,
                        };
                        _player.ForceUpdateTransform();
                        _player.ResetPhysicsInterpolation();

                        // 出口朝向设置
                        PipeTransportDir = passageOut.Direction switch {
                            PassageIn.PassageDirection.Up => PipeTransportDirection.Up,
                            PassageIn.PassageDirection.Down => PipeTransportDirection.Down,
                            PassageIn.PassageDirection.Left => PipeTransportDirection.Left,
                            PassageIn.PassageDirection.Right => PipeTransportDirection.Right,
                            _ => PipeTransportDir,
                        };

                        PipeTransportStatus = PipeTransportStatusEnum.Out;
                        
                        // 镜头控制元件检测
                        ViewControlDetect();
                        
                        // 取消滚屏状态
                        if (_levelCamera == null) {
                            GD.PushError("PipeTransport: LevelCamera is null!");
                        } else {
                            if (_levelCamera.CameraMode == LevelCamera.CameraModeEnum.AutoScroll) {
                                _levelCamera.CameraMode = LevelCamera.CameraModeEnum.FollowPlayer;
                            }
                        }
                        
                        EmitSignal(SignalName.PlaySoundPowerDown);
                    }
                }
                break;

            // 出水管
            case PipeTransportStatusEnum.Out:
                if (_player.MoveAndCollide(Vector2.Zero, true, 0.02f) == null) {
                    // 传送结束的空中跳跃处理
                    _player.Velocity = Vector2.Zero;
                    _player.MoveAndSlide();

                    if (_jump && !_player.IsOnFloor()) {
                        if (!IsInWater) {
                            Jump();
                            // MoveAndSlide 一次以防止传送前 IsOnFloor 状态遗留导致无法起跳
                            _player.Velocity = Vector2.Zero;
                            _player.MoveAndSlide();
                        } else {
                            Swim();
                        }
                    }

                    _player.ForceUpdateTransform();
                    _player.GetNode<ShapeCast2D>(_areaBodyCollision).ForceUpdateTransform();

                    // 及时更新一次重叠检测
                    // 不及时更新可能的后果：保留传送前的检测结果，导致玩家传送结束后有 1 帧时间可以进入刚才的传送入口
                    _results = ShapeQueryResult.ShapeQuery(_player, _player.GetNode<ShapeCast2D>(_areaBodyCollision));

                    _wasInPipe = true;
                    IsInPipeTransport = false;

                    // Additional Settings: MF-Style pipe exit
                    if (!LevelConfigAccess.GetLevelConfig(this).MfStylePipeExit) {
                        EmitSignal(SignalName.SetPipeOutInvincible);
                    }
                    break;
                }

                _wasPipeOut = IsInPipeTransport;

                switch (PipeTransportDir) {
                    case PipeTransportDirection.Up:
                        _player.Position -= new Vector2(0f, 0.7f);
                        // 玩家与封顶重合则强制向下挤出
                        var collide = _player.MoveAndCollide(Vector2.Zero, true, 0.02f);
                        if (collide?.GetCollider() != null) {
                            if (IsInstanceValid(collide.GetCollider())) {
                                if (collide.GetCollider() is Sealer && _player.Position.Y < -16f) {
                                    ForceDownPush();
                                }
                            }
                        }
                        break;
                    case PipeTransportDirection.Down:
                        _player.Position += new Vector2(0f, 0.7f);
                        break;
                    case PipeTransportDirection.Left:
                        _player.Position -= new Vector2(0.7f, 0f);
                        break;
                    case PipeTransportDirection.Right:
                        _player.Position += new Vector2(0.7f, 0f);
                        break;
                }
                break;
        }
    }

    // 冰块检测
    public void OnIceDetect() {
        var bodies = _onIceArea2D.GetOverlappingBodies();
        
        // 离开在冰块上状态
        if (IsOnIce) {
            // 地上要解除冰块状态必须先在地面上
            if (!IsInWater) {
                if (_player.IsOnFloor()) {
                    IsOnIce = false;
                    foreach (var body in bodies) {
                        if (body is not StaticBody2D staticBody2D) continue;
                        if (staticBody2D.HasMeta("IceBlock")) {
                            IsOnIce = true;
                            break;
                        }
                    }
                }
            }
            // 水中只要离开冰块就立刻解除状态
            else if (!_player.IsOnFloor()) {
                IsOnIce = false;
            }
        }

        // 进入在冰块上状态
        foreach (var body in bodies) {
            if (body is not StaticBody2D staticBody2D) continue;
            if (staticBody2D.HasMeta("IceBlock")) {
                IsOnIce = true;
                break;
            }
        }
    }
    public void ForceDownPush() {
        do {
            _player.Position += Vector2.Down;
        } while (_player.MoveAndCollide(Vector2.Zero, true, 0.02f) != null);
    }

    public void ViewControlDetect() {
        if (_levelCamera == null) {
            GD.PushError("ViewControlDetect: LevelCamera is null!");
            return;
        }

        var viewControls = GetTree().GetNodesInGroup("view_control");
        if (viewControls == null) return;
        if (_levelConfig == null) {
            GD.PushError("ViewControlDetect: LevelConfig is null!");
            return;
        }
        foreach (var node in viewControls) {
            if (node is not ViewControl viewControl) continue;
            if (_player.Position.X > viewControl.ViewRect.Position.X
                && _player.Position.X < viewControl.ViewRect.End.X
                && _player.Position.Y > viewControl.ViewRect.Position.Y
                && _player.Position.Y < viewControl.ViewRect.End.Y) {
                
                _levelCamera.LimitLeft = (int)viewControl.ViewRect.Position.X;
                _levelCamera.LimitTop = (int)viewControl.ViewRect.Position.Y;
                _levelCamera.LimitRight = (int)viewControl.ViewRect.End.X;
                _levelCamera.LimitBottom = (int)viewControl.ViewRect.End.Y;

                /*
                GD.Print($"_levelCamera.LimitLeft: {_levelCamera.LimitLeft}");
                GD.Print($"_levelCamera.LimitTop: {_levelCamera.LimitTop}");
                GD.Print($"_levelCamera.LimitRight: {_levelCamera.LimitRight}");
                GD.Print($"_levelCamera.LimitBottom: {_levelCamera.LimitBottom}");
                */
                
                // 同时触发场景控制元件
                viewControl.SetLevelScene();
                
                // 检测到一个镜头控制元件后即退出
                return;
            }
        }
        
        // 没有镜头控制元件时恢复默认限制
        _levelCamera.LimitLeft = 0;
        _levelCamera.LimitTop = 0;
        _levelCamera.LimitRight = (int)_levelConfig.RoomWidth;
        _levelCamera.LimitBottom = (int)_levelConfig.RoomHeight;
    }
    public void AutoScrollDetect() {
        if (_levelCamera == null) {
            GD.PushError("AutoScrollDetect: LevelCamera is null!");
            return;
        }
        
        // 检测第一个滚屏节点
        var autoScrollNode = GetTree().GetFirstNodeInGroup("auto_scroll");
        if (autoScrollNode == null) return;
        if (autoScrollNode is not AutoScroll autoScroll) return;
        if (IsPlayerInAutoScrollNodeRect(autoScroll)) {
            _levelCamera.CameraMode = LevelCamera.CameraModeEnum.AutoScroll;
            if (!_levelStartAutoScrollDetect) {
                // 起始点在自动卷轴范围内，设置位置为滚屏节点
                _levelCamera.Position = autoScroll.Position;
                _levelCamera.ResetPhysicsInterpolation();
                _levelStartAutoScrollDetect = true;
            }
            _levelStartAutoScrollDetect = true;
            // 镜头限制取消
            CameraLimitFree();
        }
        
        // Checkpoint 处复活，检测周围滚屏节点，只检测一次
        if (!_autoScrollCheckpointDetect || _autoScrollCheckpointDetected) return;
        _autoScrollCheckpointDetected = true;
        var autoScrollNodes = GetTree().GetNodesInGroup("auto_scroll");
        foreach (var node in autoScrollNodes) {
            if (node is not AutoScroll autoScrollCheckpoint) continue;
            if (IsPlayerInAutoScrollNodeRect(autoScrollCheckpoint)) {
                _levelCamera.CameraMode = LevelCamera.CameraModeEnum.AutoScroll;
                // 强行设置强制滚屏的节点
                _levelCamera.ForceSetScrollNode(autoScrollCheckpoint);
                // 并设置位置为滚屏节点
                _levelCamera.Position = autoScrollCheckpoint.Position;
                _levelCamera.ResetPhysicsInterpolation();
                //GD.Print($"Current CP Auto Scroll Node: {autoScrollCheckpoint.Name}");
                // 镜头限制取消
                CameraLimitFree();
                // 检测到一个就停止
                break;
            }
        }
    }
    public bool IsPlayerInAutoScrollNodeRect(AutoScroll autoScroll) {
        return _player.Position.X > autoScroll.ScrollRect.Position.X
               && _player.Position.X < autoScroll.ScrollRect.End.X
               && _player.Position.Y > autoScroll.ScrollRect.Position.Y
               && _player.Position.Y < autoScroll.ScrollRect.End.Y;
    }

    public void KoopaScrollDetect() {
        if (_levelCamera == null) {
            GD.PushError("KoopaScrollDetect: LevelCamera is null!");
            return;
        }
        
        // 检测第一个滚屏节点
        if (_levelCamera.CameraMode == LevelCamera.CameraModeEnum.Koopa) return;
        var koopaScrollNode = GetTree().GetFirstNodeInGroup("koopa_scroll");
        if (koopaScrollNode == null) return;
        if (koopaScrollNode is not KoopaScroll koopaScroll) return;
        var screen = ScreenUtils.GetScreenRect(this);
        var screenCenterX = screen.Position.X + screen.Size.X / 2f;
        if (screenCenterX > koopaScroll.GlobalPosition.X - koopaScroll.ScrollTriggerDistance
            && screenCenterX < koopaScroll.GlobalPosition.X + koopaScroll.ScrollTriggerDistance) {
            _levelCamera.CameraMode = LevelCamera.CameraModeEnum.Koopa;
            
            // 设置默认音乐
            koopaScroll.SetBgm();
            
            // 设置场景控制元件
            koopaScroll.SetLevelScene();
            
            // 激活库巴血条 HUD 显示
            var koopaHudNode = (KoopaHUD)GetTree().GetFirstNodeInGroup("koopa_hud_node2d");
            koopaHudNode.Activate = true;
            
            // 自动滚屏会自动取消
            
            // 镜头限制取消
            CameraLimitFree();
        }
    }

    public void CameraLimitFree() {
        if (_levelConfig == null) {
            GD.PushError("KoopaScrollDetect: LevelConfig is null!");
            return;
        }
        if (_levelCamera == null) {
            GD.PushError("KoopaScrollDetect: LevelCamera is null!");
            return;
        }
            
        _levelCamera.LimitLeft = 0;
        _levelCamera.LimitTop = 0;
        _levelCamera.LimitRight = (int)_levelConfig.RoomWidth;
        _levelCamera.LimitBottom = (int)_levelConfig.RoomHeight;
    }
}
