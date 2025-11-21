using Godot;
using System;
using SMWP.Level.Enemy;

public partial class ShellStaticInteraction : GoombaEnemyInteraction {
    public override void PlayerHurtCheck(bool check) {
        EmitSignal(EnemyInteraction.SignalName.PlaySoundStomped);
        EmitSignal(GoombaEnemyInteraction.SignalName.GoombaStomped, Variant.From(EnemyDieType));
    }
}
