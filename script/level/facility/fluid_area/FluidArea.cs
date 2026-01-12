using Godot;

public partial class FluidArea : Area2D {
    [Export] public CollisionShape2D CollisionShape = null!;
    [Export] public Rect2 FluidRect = new Rect2(Vector2.Zero, new Vector2(32f, 32f));
    [Export] public float TargetHeight;
    [Export] public float Speed = 1f;
    
    /// <summary>
    /// 暂未使用
    /// </summary>
    [Export] public bool Reusable { get; set; }
    
    private Water? _water;
    private bool _set;

    public override void _Ready() {
        _water ??= (Water)GetTree().GetFirstNodeInGroup("water_global");
        CollisionShape.GlobalPosition = GlobalPosition + FluidRect.Size / 2;
        var shapeResource = CollisionShape.Shape;
        var shape = (RectangleShape2D)shapeResource.Duplicate();
        shape.Size = FluidRect.Size;
        CollisionShape.Shape = shape;
    }
    public override void _PhysicsProcess(double delta) {
        if (GetOverlappingBodies().Count <= 0) return;
        SetFluidMovement();
    }
    public void OnBodyEntered(Node2D body) {
        SetFluidMovement();
    }

    public void SetFluidMovement() {
        // 防止 GetOverlappingBodies 和 OnBodyEntered 方法保守并用而重复触发
        if (_set) return;
        if (!_set) _set = true;
        
        if (_water == null) {
            GD.PushError($"{this}: _water is null!");
            return;
        }
        _water.FluidControlSet(TargetHeight, Speed);

        if (Reusable) {
            _set = false;
        } else {
            QueueFree();   
        }
    }
}
