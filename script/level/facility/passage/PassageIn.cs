using Godot;
using System;

public partial class PassageIn : Node2D {
    [Export] public int PassageId;
    
    [Export] private PackedScene _passageInUp = null!;
    [Export] private PackedScene _passageInDown = null!;
    [Export] private PackedScene _passageInLeft = null!;
    [Export] private PackedScene _passageInRight = null!;
    
    public enum PassageDirection {
        Up,
        Down,
        Left,
        Right,
    }
    [Export] public PassageDirection Direction = PassageDirection.Up;
    
    public override void _Ready() {
        Callable.From(() => {
            Area2D passageInArea2D = Direction switch {
                PassageDirection.Up => (Area2D)_passageInUp.Instantiate(),
                PassageDirection.Down => (Area2D)_passageInDown.Instantiate(),
                PassageDirection.Left => (Area2D)_passageInLeft.Instantiate(),
                PassageDirection.Right => (Area2D)_passageInRight.Instantiate(),
                _ => throw new ArgumentOutOfRangeException(),
            };
            AddChild(passageInArea2D);
            passageInArea2D.GlobalPosition = GlobalPosition;
            // 元数据应当设置给重叠检测对象以让 PlayerInteraction 识别
            passageInArea2D.SetMeta("PipeEntry", this);
        }).CallDeferred();
    }
}
