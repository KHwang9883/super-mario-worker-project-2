using Godot;
using System;

public interface IStompable {
    float StompOffset => -12f;

    public void Stomped(Node2D stomper);
}
