namespace SMWP.Level.Interface;

public interface IHurtableAndKillable {
    enum HurtEnum {
        Hurt,
        Die
    }

    HurtEnum HurtType => HurtEnum.Hurt;
}