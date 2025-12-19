using Godot;
using System;
using Godot.Collections;
using SMWP.Util;

[Tool]
public partial class CheepCheepArea : Area2D {
    [Export] public Rect2 CheepAreaRect;
    
    public enum CheepAreaTypeEnum { Swim, Fly }
    [Export] public CheepAreaTypeEnum CheepAreaType;
    public enum CheepAreaLevelEnum { Level1, Level2 }
    [Export] public CheepAreaLevelEnum CheepAreaLevel;
    public enum CheepAreaDirectionEnum { Left, Right }
    [Export] public CheepAreaDirectionEnum CheepAreaDirection;
    public enum CheepTypeEnum { Red, Blue, Green, Spike }
    [Export] public CheepTypeEnum CheepType;
    
    [Export] private Dictionary<CheepTypeEnum, PackedScene> _cheepDict = new() {
        { CheepTypeEnum.Red, GD.Load<PackedScene>("uid://b3g6gfkyvkrd5") },
        { CheepTypeEnum.Blue, GD.Load<PackedScene>("uid://trieosghwihq") },
        { CheepTypeEnum.Green, GD.Load<PackedScene>("uid://be1fapu1uxogc") },
        { CheepTypeEnum.Spike, GD.Load<PackedScene>("uid://b2252u3w05f1u") },
    };
    
    private Node2D? _waterGlobal;
    private bool _isPlayerIn;
    private CollisionShape2D? _collisionShape2D;
    private int _timer;
    private Node2D? _cheep;
    private RandomNumberGenerator _rng = new();
    private bool _cheepSwimCreateHeightShift;

    public override void _Ready() {
        _waterGlobal ??= (Node2D)GetTree().GetFirstNodeInGroup("water_global");
        _collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
        _collisionShape2D.GlobalPosition = CheepAreaRect.Position + CheepAreaRect.Size / 2f;
        var rectangleShape2D = (RectangleShape2D)_collisionShape2D.Shape;
        rectangleShape2D.Size = CheepAreaRect.Size;
    }
    public override void _PhysicsProcess(double delta) {
        // 编辑器内矩形区域预览
        if (Engine.IsEditorHint()) {
            Position = Vector2.Zero;
            _collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
            _collisionShape2D.GlobalPosition = CheepAreaRect.Position + CheepAreaRect.Size / 2f;
            var rectangleShape2D = (RectangleShape2D)_collisionShape2D.Shape;
            rectangleShape2D.Size = CheepAreaRect.Size;
            return;
        }
        
        if (!_isPlayerIn) return;
        
        _timer++;
        
        switch (CheepAreaType) {
            case CheepAreaTypeEnum.Swim:
                
                switch (CheepAreaLevel) {
                    case CheepAreaLevelEnum.Level1:
                        
                        switch (CheepType) {
                            case CheepTypeEnum.Red:
                                if (_timer == 50) Create(8);
                                if (_timer == 100) {
                                    _timer = 0;
                                    Create(8);
                                }
                                break;
                            case CheepTypeEnum.Green:
                                if (_timer == 50) Create(10);
                                if (_timer == 100) {
                                    _timer = 0;
                                    Create(10);
                                }
                                break;
                        }
                        break;
                    
                    case CheepAreaLevelEnum.Level2:
                        
                        switch (CheepType) {
                            case CheepTypeEnum.Red:
                                if (_timer == 38) Create(10);
                                if (_timer == 75) {
                                    _timer = 0;
                                    Create(10);
                                }
                                break;
                        }
                        break;
                }
                break;
            
            case CheepAreaTypeEnum.Fly:
                
                switch (CheepAreaLevel) {
                    case CheepAreaLevelEnum.Level1:
                        switch (CheepType) {
                            case CheepTypeEnum.Red:
                                switch (CheepAreaDirection) {
                                    case CheepAreaDirectionEnum.Left:
                                        if (_timer == 13) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                    case CheepAreaDirectionEnum.Right:
                                        if (_timer == 25) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                }
                                break;
                            case CheepTypeEnum.Blue:
                                switch (CheepAreaDirection) {
                                    case CheepAreaDirectionEnum.Left:
                                        if (_timer == 13) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                    case CheepAreaDirectionEnum.Right:
                                        if (_timer == 25) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                    case CheepAreaLevelEnum.Level2:
                        switch (CheepType) {
                            case CheepTypeEnum.Red:
                                switch (CheepAreaDirection) {
                                    case CheepAreaDirectionEnum.Left:
                                        if (_timer == 4) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                    case CheepAreaDirectionEnum.Right:
                                        if (_timer == 10) {
                                            _timer = 0;
                                            Create(3);
                                        }
                                        break;
                                }
                                break;
                        }
                        break;
                }
                break;
        }
    }
    public void OnBodyEntered(Node2D body) {
        if (body.IsInGroup("player")) {
            _isPlayerIn = true;
        }
    }
    public void OnBodyExited(Node2D body) {
        if (body.IsInGroup("player")) {
            _isPlayerIn = false;
        }
    }
    public void Create(int maxCount = 3) {
        if (!_cheepDict.TryGetValue(CheepType, out var cheepScene)) return;
        
        var screen = ScreenUtils.GetScreenRect(this);
        var screenCenterX = screen.Position.X + (screen.Size.X / 2f);
        var direction = CheepAreaDirection switch {
            CheepAreaDirectionEnum.Left => 1,
            CheepAreaDirectionEnum.Right => -1,
            _ => -1,
        };
        var waterGlobal = (Node2D)GetTree().GetFirstNodeInGroup("water_global");
        switch (CheepAreaType) {
            case CheepAreaTypeEnum.Swim:
                if (GetTree().GetNodeCountInGroup("CreatedCheepCheepSwim") >= maxCount) return;
                _cheep = cheepScene.Instantiate<Node2D>();
                _cheep.AddToGroup("CreatedCheepCheepSwim");
                _cheep.Position = new Vector2(
                    screenCenterX + (screen.Size.X / 2f + 34f) * direction,
                    Mathf.Max(screen.Position.Y - 60f, waterGlobal.GlobalPosition.Y+38) + _rng.RandfRange(0f, 300f) + ((_cheepSwimCreateHeightShift) ? 300f : 0f)
                    );
                _cheepSwimCreateHeightShift = !_cheepSwimCreateHeightShift;
                break;
            case CheepAreaTypeEnum.Fly:
                if (GetTree().GetNodeCountInGroup("CreatedCheepCheepFly") >= maxCount) return;
                _cheep = cheepScene.Instantiate<Node2D>();
                _cheep.AddToGroup("CreatedCheepCheepFly");
                _cheep.Position = new Vector2(
                    screenCenterX + (screen.Size.X / 2f + 34f) * direction,
                    screen.End.Y + 22f + _rng.RandfRange(0f, 300f));
                break;
        }
        AddSibling(_cheep);

        if (_cheep == null) return;
        // 方向设置
        if (!_cheep.HasMeta("CheepCheepMovement")) return;
        var cheepCheepMovement = (CheepCheepMovement)_cheep.GetMeta("CheepCheepMovement");
        cheepCheepMovement.CheepMoveMode = CheepAreaType switch {
            CheepAreaTypeEnum.Swim => CheepCheepMovement.CheepCheepMoveEnum.Swim,
            CheepAreaTypeEnum.Fly => CheepCheepMovement.CheepCheepMoveEnum.Fly,
            _ => CheepCheepMovement.CheepCheepMoveEnum.Swim,
        };
        cheepCheepMovement.SpeedX = CheepAreaDirection switch {
            CheepAreaDirectionEnum.Left => -Mathf.Abs(cheepCheepMovement.SpeedX),
            CheepAreaDirectionEnum.Right => Mathf.Abs(cheepCheepMovement.SpeedX),
            _ => cheepCheepMovement.SpeedX,
        };
    }
}
