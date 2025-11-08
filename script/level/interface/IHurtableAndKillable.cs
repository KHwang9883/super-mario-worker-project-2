namespace SMWP.Level.Interface;

public interface IHurtableAndKillable {
    enum HurtEnum {
        Hurt,
        Die,
        Nothing,
    }
    HurtEnum HurtType { get; set; }
}