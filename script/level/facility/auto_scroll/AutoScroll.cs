using Godot;
using System;

public partial class AutoScroll : Node2D {
    [Export] public float Speed = 100f;
    public Rect2 ScrollRect;
    public int Id;

    public override void _Ready() {
        // 强制滚屏速度限制
        Speed = Mathf.Clamp(Speed, 0f, 6000f);
        ScrollRect = new Rect2(Position - new Vector2(320, 240), new Vector2(640, 480));
        AddToGroup("auto_scroll");
        
        Visible = false;
    }
}
