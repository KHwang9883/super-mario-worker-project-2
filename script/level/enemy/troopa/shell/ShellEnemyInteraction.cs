using Godot;
using System;
using SMWP.Level.Enemy;
using SMWP.Level.Physics;
using SMWP.Level.Sound;

public partial class ShellEnemyInteraction : EnemyInteraction {
    [Signal]
    public delegate void ShellStompedEventHandler(EnemyDie.EnemyDieEnum enemyDieEnum, bool shellSet = false, bool shellDir = false);
    public EnemyDie.EnemyDieEnum EnemyDieType = EnemyDie.EnemyDieEnum.CreateInstance;

    private bool _shellDir;
    
    public override float OnStomped(Node2D stomper) {
        EmitSignal(EnemyInteraction.SignalName.Stomped);
        if (ImmuneToStomp) {
            EmitSignal(EnemyInteraction.SignalName.PlaySoundBumped);
            return StompSpeedY;
        }
        EmitSignal(EnemyInteraction.SignalName.PlaySoundStomped);
        OnNormalAddScore();
        // 被踩以后生成龟壳被踩的类型的尸体，外加方向参数
        var basicMovementComponent = GetParent().GetNodeOrNull<BasicMovement>("BasicMovement");
        if (basicMovementComponent != null) {
            _shellDir = Math.Sign(basicMovementComponent.SpeedX) > 0;
        }
        EmitSignal(SignalName.ShellStomped, Variant.From(EnemyDieType), true, _shellDir);
        //OnDied();
        return StompSpeedY;
    }
}
