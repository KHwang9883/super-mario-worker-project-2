using Godot;

namespace SMWP.Level.Interface;

public interface IStompable {
    [Signal]
    delegate void OnStompedEventHandler(Node2D stomper);
    float StompOffset => -12f;

    public void OnStomped(Node2D stomper);
}