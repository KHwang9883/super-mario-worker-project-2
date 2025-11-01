using Godot;
using System;

public interface IStompable {
    [Signal]
    delegate void OnStompedEventHandler(Node2D stomper);
    float StompOffset => -12f;

    public void Stomped(Node2D stomper);
}
