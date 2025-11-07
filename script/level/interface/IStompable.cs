using Godot;

namespace SMWP.Level.Interface;

public interface IStompable {
    float StompOffset { get; set; }
    float StompSpeedY { get; set; }

    public float OnStomped(Node2D stomper);
}