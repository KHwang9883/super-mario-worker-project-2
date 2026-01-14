using Godot;
using System;
using Godot.Collections;
using SMWP.Level;

public partial class LevelCamera : Camera2D {
    private uint _originCollisionLayer;
    
    public enum CameraModeEnum {
        FollowPlayer,
        AutoScroll,
        Koopa,
    }
    public CameraModeEnum CameraMode = CameraModeEnum.FollowPlayer;
    
    private LevelConfig? _levelConfig;
    private CharacterBody2D? _player;
    
    private Vector2 _originalPosition;
    public Vector2 DeltaPosition;
    public bool InitializationCompleted;
    
    // Force Scroll
    public bool ForceScrollDisabled;
    
    // Auto Scroll
    private Array<AutoScroll> _autoScrollNodes = [];
    private AutoScroll? _targetAutoScrollNode;
    private int _autoScrollIndex = 0;
    private float _autoScrollSpeed = 100f;
    public bool AutoScrollEnded;
    
    // Koopa Scroll
    private Array<KoopaScroll> _koopaScrollNodes = [];
    private KoopaScroll? _targetKoopaScrollNode;
    private int _koopaScrollIndex = 0;
    private float _koopaScrollSpeed = 100f;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        LimitRight = (int)_levelConfig.RoomWidth;
        LimitBottom = (int)_levelConfig.RoomHeight;
        _player ??= (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        
        // 设置到玩家位置
        //Position = _player.Position;
        //ResetPhysicsInterpolation();
        
        Callable.From(() => {
            // 设置到玩家位置
            Position = _player.Position;
            ResetPhysicsInterpolation();
            
            // 获取第一个滚屏节点
            
            // 强制滚屏节点组获取
            _targetAutoScrollNode = GetTree().GetFirstNodeInGroup("auto_scroll") as AutoScroll;
            if (_targetAutoScrollNode != null) {
                _autoScrollSpeed = _targetAutoScrollNode.Speed;

                var originIndex = _autoScrollIndex;
                foreach (var node in GetTree().GetNodesInGroup("auto_scroll")) {
                    if (node is not AutoScroll autoScroll) continue;
                    autoScroll.Id = _autoScrollIndex;
                    _autoScrollIndex++;
                    _autoScrollNodes.Add(autoScroll);
                }
                _autoScrollIndex = originIndex;
            }
            
            // 库巴滚屏节点获取
            _targetKoopaScrollNode = GetTree().GetFirstNodeInGroup("koopa_scroll") as KoopaScroll;
            
            _originalPosition = Position;
        }).CallDeferred();
    }
    public override void _PhysicsProcess(double delta) {
        Callable.From(() => {
            if (!InitializationCompleted) InitializationCompleted = true;
        }).CallDeferred();
        
        // 不在自动滚屏模式下，恢复目标滚屏节点为第一个节点
        // 不在库巴滚屏模式下，恢复目标滚屏节点为第一个节点
        if (CameraMode == CameraModeEnum.FollowPlayer) {
            _targetAutoScrollNode = GetTree().GetFirstNodeInGroup("auto_scroll") as AutoScroll;
            if (_targetAutoScrollNode != null) {
                _autoScrollSpeed = _targetAutoScrollNode.Speed;
                _autoScrollIndex = 0;
                ForceScrollDisabled = false;
            }
            
            _targetKoopaScrollNode = GetTree().GetFirstNodeInGroup("koopa_scroll") as KoopaScroll;
            if (_targetKoopaScrollNode != null) {
                _koopaScrollSpeed = _targetKoopaScrollNode.Speed;
                _koopaScrollIndex = 0;
                ForceScrollDisabled = false;
            }
        }

        if (CameraMode != CameraModeEnum.FollowPlayer) {
            // 强制滚屏下玩家死亡后滚屏停止运动
            if (_player is { ProcessMode: ProcessModeEnum.Disabled}) {
                return;
            }
        }
        
        switch (CameraMode) {
            case CameraModeEnum.FollowPlayer:
                if (_player == null) {
                    GD.PushError($"{this}: Player is null!");
                } else {
                    Position = _player.Position.Round();
                }
                break;
            
            case CameraModeEnum.AutoScroll:
                if (_targetAutoScrollNode == null) {
                    //GD.PushError($"{this}: Target autoscroll node is null!");
                    break;
                }
                
                var autoSpeed = _autoScrollSpeed * 0.01f;
                if (ForceScrollDisabled) autoSpeed = 0f;
                
                if (!ForceScrollDisabled) AutoScrollEnded = false;
                
                Position += (_targetAutoScrollNode.Position - Position).Normalized() * autoSpeed;
                if (Math.Abs(Position.X - _targetAutoScrollNode.Position.X) < autoSpeed
                    && Math.Abs(Position.Y - _targetAutoScrollNode.Position.Y) < autoSpeed) {
                    
                    if (_autoScrollIndex < _autoScrollNodes.Count) {
                        _autoScrollIndex++;
                    }
                    
                    // 切换到下一个滚屏节点
                    Position = _targetAutoScrollNode.Position;
                    _autoScrollSpeed = _targetAutoScrollNode.Speed;
                    _targetAutoScrollNode =
                        _autoScrollNodes[Mathf.Clamp(_autoScrollIndex, 0, _autoScrollNodes.Count - 1)];
                    
                    if (_autoScrollIndex >= _autoScrollNodes.Count) {
                        // 滚屏终止
                        _autoScrollIndex = 0;
                        Position = _targetAutoScrollNode.Position;
                        _targetAutoScrollNode = GetTree().GetFirstNodeInGroup("auto_scroll") as AutoScroll;
                        if (_targetAutoScrollNode != null) {
                            _autoScrollSpeed = _targetAutoScrollNode.Speed;
                        }
                        ForceScrollDisabled = true;
                        AutoScrollEnded = true;
                    }
                }
                break;
            
            case CameraModeEnum.Koopa:
                if (_player == null) {
                    GD.PushError($"{this}: Player is null!");
                    return;
                }
                
                // 玩家 y 位置跟踪正常
                Position = Position with { Y = _player.Position.Y };
                // 如果最后一只库巴中途掉崖销毁，滚屏停止
                // 如果非最后一只库巴中途掉崖销毁，直接获取下一个节点
                if (!IsInstanceValid(_targetKoopaScrollNode)) {
                    _targetKoopaScrollNode = GetTree().GetFirstNodeInGroup("koopa_scroll") as KoopaScroll;
                    //GD.Print($"Current koopa scroll node is: {_targetKoopaScrollNode}");
                    if (_targetKoopaScrollNode != null) {
                        //GD.Print("非最后一只库巴中途掉崖销毁，直接获取下一个节点");
                        _targetKoopaScrollNode.SetBgm();
                        break;
                    } else {
                        //GD.Print("最后一只库巴中途掉崖销毁，滚屏停止");
                        ForceScrollDisabled = true;
                        break;
                    }
                }
                
                // 滚屏运动
                var koopaSpeed =
                    Mathf.Sign(_targetKoopaScrollNode.ScrollPosX - Position.X) * _koopaScrollSpeed;
                if (ForceScrollDisabled) koopaSpeed = 0f;
                Position = Position with { X = Position.X + koopaSpeed };
                
                // 滚屏到位对齐
                if (Math.Abs(Position.X - _targetKoopaScrollNode.ScrollPosX) < koopaSpeed) {
                    Position = Position with { X = _targetKoopaScrollNode.ScrollPosX };
                }
                break;
        }
        
        // 位移过大时直接重置物理插值状态
        DeltaPosition = Position - _originalPosition;
        if ((DeltaPosition).Length() > 28 /*800f*/) {
            ResetPhysicsInterpolation();
        }
        _originalPosition = Position;
    }

    // Auto Scroll 强制设置节点（用于 Checkpoint 处复活设置）
    public void ForceSetScrollNode(AutoScroll autoScroll) {
        Position = autoScroll.Position;
        ResetPhysicsInterpolation();
        _targetAutoScrollNode = autoScroll;
        _autoScrollIndex = autoScroll.Id;
    }
}
