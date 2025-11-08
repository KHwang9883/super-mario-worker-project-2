using Godot;

namespace SMWP.Level.Interface;

public interface IStompable {
    bool Stompable { get; set; }
    float StompOffset { get; set; }
    float StompSpeedY { get; set; }

    public float OnStomped(Node2D stomper);
}