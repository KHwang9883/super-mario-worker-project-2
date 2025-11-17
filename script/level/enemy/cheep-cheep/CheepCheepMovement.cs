using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Physics;

public partial class CheepCheepMovement : BasicMovement {
    public enum CheepCheepMoveEnum {
        Swim,
        Fly,
    }
    [Export] public CheepCheepMoveEnum CheepMoveMode = CheepCheepMoveEnum.Swim;
    [Export] private Area2D? _area2D;
    [Export] private EnemyInteraction? _enemyInteraction;
    [Export] private bool _stompableOutOfWater = true;
    
    private bool _isInWater;
    private bool _flewOutOfWater;
    private bool _flyMode;
    private RandomNumberGenerator _rng = new();
    
    public override void _Ready() {
        base._Ready();
        MoveObject.SetMeta("CheepCheepMovement", this);
        if (CheepMoveMode == CheepCheepMoveEnum.Fly) _flyMode = true;
    }
    public override void _PhysicsProcess(double delta) {
        if (_area2D == null) return;
        
        if (!_flewOutOfWater && _flyMode) {
            // 入水标记
            foreach (var area in _area2D.GetOverlappingAreas()) {
                if (!area.IsInGroup("water")) continue;
                _isInWater = true;
            }
            
            // 入水标记后的出水检测
            if (_isInWater) {
                CheepMoveMode = CheepCheepMoveEnum.Fly;
                foreach (var area in _area2D.GetOverlappingAreas()) {
                    if (!area.IsInGroup("water")) continue;
                    CheepMoveMode = CheepCheepMoveEnum.Swim;
                }
            }
        }

        switch (CheepMoveMode) {
            case CheepCheepMoveEnum.Swim:
                SwimMove(delta);
                break;
            case CheepCheepMoveEnum.Fly:
                FlyMove(delta);
                break;
        }
    }

    public virtual void SwimMove(double delta) {
        TurnDetect();
        
        SpeedXProcess();
        
        ApplySpeed();

        Move();
    }
    public virtual void FlyMove(double delta) {
        // 飞行跳跃初始化设置
        if (!_flewOutOfWater) {
            _flewOutOfWater = true;
            
            // 不参与与地面实心的判定（针对蓝鱼情形）
            MoveObject.SetCollisionMask(0);
        
            SpeedX = 4f - _rng.RandiRange(0, 4) + 1.25f;
            SpeedY = _rng.RandiRange(0, 10) * -1.3f;
            if (InitiallyFaceToPlayer) {
                SetMovementDirection();
            }
        }
        
        // 可踩性设置
        if (_enemyInteraction != null) _enemyInteraction.Stompable = _stompableOutOfWater;
        
        base._PhysicsProcess(delta);
    }
    public void OnScreenExited() {
        if (_flewOutOfWater && MoveObject.IsInGroup("CreatedCheepCheepFly")) {
            MoveObject.RemoveFromGroup("CreatedCheepCheepFly");
        }
    }
}
