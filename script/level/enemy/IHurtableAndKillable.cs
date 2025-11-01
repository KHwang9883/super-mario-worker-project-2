using Godot;

namespace SMWP.Level.Enemy;

public interface IHurtableAndKillable {
    enum HurtEnum {
        Hurt,
        Die
    }

    HurtEnum HurtType => HurtEnum.Hurt;
}