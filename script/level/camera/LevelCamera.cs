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
    private Array<AutoScroll> _autoScrollNodes = [];
    private AutoScroll? _targetAutoscrollNode;
    private int _autoScrollIndex = 0;
    private float _autoScrollSpeed = 100f;
    public bool AutoScrollDisabled;
    private Vector2 _originalPosition;
    public Vector2 DeltaPosition;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        LimitRight = (int)_levelConfig.RoomWidth;
        LimitBottom = (int)_levelConfig.RoomHeight;
        _player ??= (CharacterBody2D)GetTree().GetFirstNodeInGroup("player");
        
        Callable.From(() => {
            // 获取第一个滚屏节点
            _targetAutoscrollNode = (AutoScroll)GetTree().GetFirstNodeInGroup("auto_scroll");
            _autoScrollSpeed = _targetAutoscrollNode.Speed;
            
            foreach (var node in GetTree().GetNodesInGroup("auto_scroll")) {
                if (node is AutoScroll autoScroll) {
                    _autoScrollNodes.Add(autoScroll);
                }
            }
        }).CallDeferred();
        
        _originalPosition = Position;
    }
    public override void _PhysicsProcess(double delta) {
        // 不在自动滚屏模式下，恢复目标滚屏节点为第一个节点
        if (CameraMode != CameraModeEnum.AutoScroll) {
            _targetAutoscrollNode = (AutoScroll)GetTree().GetFirstNodeInGroup("auto_scroll");
            _autoScrollSpeed = _targetAutoscrollNode.Speed;
            _autoScrollIndex = 0;
            AutoScrollDisabled = false;
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
                if (_targetAutoscrollNode == null) {
                    GD.PushError($"{this}: Target autoscroll node is null!");
                    break;
                }
                
                var speed = _autoScrollSpeed * 0.01f;
                if (AutoScrollDisabled) speed = 0f;
                Position += (_targetAutoscrollNode.Position - Position).Normalized() * speed;
                if (Math.Abs(Position.X - _targetAutoscrollNode.Position.X) < speed
                    && Math.Abs(Position.Y - _targetAutoscrollNode.Position.Y) < speed) {
                    
                    if (_autoScrollIndex < _autoScrollNodes.Count) {
                        _autoScrollIndex++;
                    }
                    
                    // 切换到下一个滚屏节点
                    Position = _targetAutoscrollNode.Position;
                    _autoScrollSpeed = _targetAutoscrollNode.Speed;
                    _targetAutoscrollNode =
                        _autoScrollNodes[Mathf.Clamp(_autoScrollIndex, 0, _autoScrollNodes.Count - 1)];
                    
                    if (_autoScrollIndex >= _autoScrollNodes.Count) {
                        // 滚屏终止
                        _autoScrollIndex = 0;
                        Position = _targetAutoscrollNode.Position;
                        _targetAutoscrollNode = (AutoScroll)GetTree().GetFirstNodeInGroup("auto_scroll");
                        _autoScrollSpeed = _targetAutoscrollNode.Speed;
                        AutoScrollDisabled = true;
                    }
                }
                break;
            
            case CameraModeEnum.Koopa:
                // Todo: 库巴滚屏
                break;
        }
        
        // 位移过大时直接重置物理插值状态
        DeltaPosition = Position - _originalPosition;
        if ((DeltaPosition).Length() > 800f) {
            ResetPhysicsInterpolation();
        }
        _originalPosition = Position;
    }
}
