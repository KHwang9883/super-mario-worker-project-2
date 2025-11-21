using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Sound;

public partial class GoombaEnemyInteraction : EnemyInteraction {
    [Signal]
    public delegate void GoombaStompedEventHandler(EnemyDie.EnemyDieEnum enemyDieEnum);
    public EnemyDie.EnemyDieEnum EnemyDieType = EnemyDie.EnemyDieEnum.CreateInstance;
    
    public override float OnStomped(Node2D stomper) {
        EmitSignal(EnemyInteraction.SignalName.Stomped);
        if (ImmuneToStomp) {
            EmitSignal(EnemyInteraction.SignalName.PlaySoundBumped);
            return StompSpeedY;
        }
        EmitSignal(EnemyInteraction.SignalName.PlaySoundStomped);
        OnNormalAddScore();
        // 被踩以后生成板栗仔被踩的类型的尸体
        EmitSignal(SignalName.GoombaStomped, Variant.From(EnemyDieType));
        //OnDied();
        return StompSpeedY;
    }
}
