using Godot;
using System;

public partial class BonusSprout : Node {
    private CollisionObject2D? _parent;
    public bool Overlapping;
    private int _detectedDelayTimer;
    public bool Detected;

    public override void _Ready() {
        _parent ??= GetParent<CollisionObject2D>();
    }
    public override void _PhysicsProcess(double delta) {
        // 因为延迟问题不做初始时没有与墙体重叠则组件禁用
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
        if (Detected) return;
        
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
            
            // 如果不与墙体重叠，则不再检测；并且检测时考虑重叠检测延迟的问题
            if (_detectedDelayTimer < 10) {
                _detectedDelayTimer++;
            } else {
                if (!Overlapping) Detected = true;
            }
        }
    }
}
