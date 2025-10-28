using Godot;
using System;

namespace SMWP.Level.Enemy;

public partial class EnemyBase : Node2D {
    public enum StompabilityEnum {
        Stompable,
        Unstompable
    }
    [Export] public StompabilityEnum Stompability = StompabilityEnum.Stompable;
    public enum HurtEnum {
        Hurt,
        Die
    }
    [Export] public HurtEnum HurtType = HurtEnum.Hurt;
}