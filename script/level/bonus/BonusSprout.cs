using Godot;
using System;

public partial class BonusSprout : Node {
    private CollisionObject2D? _parent;
    public bool Overlapping;

    public override void _Ready() {
        _parent ??= GetParent<CollisionObject2D>();
        OverlapDetect();
        
        // 初始时没有与墙体重叠则组件禁用
        if (!Overlapping) {
            ProcessMode = ProcessModeEnum.Disabled;
            GD.Print("what's up");
        }
    }
    public override void _PhysicsProcess(double delta) {
        OverlapDetect();
        if (_parent == null) {
            GD.PushError($"{this}: _parent is null!");
        } else {
            if (Overlapping) {
                _parent.Position += Vector2.Up;
            }
        }
    }

    public void OverlapDetect() {
        if (_parent == null) {
            GD.PushError($"{this}: _parent is null!");
        } else {
            Overlapping = false;
            
            switch (_parent) {
                case Area2D area: {
                    if (area.GetOverlappingBodies().Count > 0) {
                        Overlapping = true;
                    }
                    break;
                }
                
                case CharacterBody2D character: {
                    if (character.MoveAndCollide(Vector2.Zero, true, 0.02f) != null) {
                        Overlapping = true;
                    }
                    break;
                }
            }
        }
    }
}
