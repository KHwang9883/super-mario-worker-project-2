using Godot;
using System;

public partial class FluidArea : Area2D {
    [Export] public CollisionShape2D CollisionShape = null!;
    [Export] public Rect2 FluidRect = new Rect2(Vector2.Zero, new Vector2(32f, 32f));
    [Export] public float TargetHeight;
    [Export] public float Speed = 1f;
    
    private Water? _water;

    public override void _Ready() {
        _water ??= (Water)GetTree().GetFirstNodeInGroup("water_global");
        CollisionShape.GlobalPosition = GlobalPosition + FluidRect.Size / 2;
        var shape = (RectangleShape2D)CollisionShape.Shape;
        shape.Size = FluidRect.Size;
    }
    public override void _PhysicsProcess(double delta) {
        if (GetOverlappingBodies().Count <= 0) return;
        SetFluidMovement();
    }
    public void OnBodyEntered(Node2D body) {
        SetFluidMovement();
    }

    public void SetFluidMovement() {
        if (_water == null) {
            GD.PushError($"{this}: _water is null!");
            return;
        }
        _water.FluidControlSet(TargetHeight, Speed);
        
        QueueFree();
    }
}
