using Godot;

namespace SMWP.Level.Interface;

public interface IStompable {
    bool Stompable { get; set; }
    float StompOffset { get; set; }
    float StompSpeedY { get; set; }

    public void MetadataInject(Node2D parent);
    public float OnStomped(Node2D stomper);
}