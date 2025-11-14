using Godot;

namespace SMWP.Level.Interface;

public interface IHurtableAndKillable {
    enum HurtEnum {
        Hurt,
        Die,
        Nothing,
    }
    HurtEnum HurtType { get; set; }
    public void MetadataInject(Node2D parent);
    public void PlayerHurtCheck(bool check);
}