using Godot;

namespace SMWP.Level.Interface;

public interface IFireballHittable {
    public void OnFireballHit(Node2D fireball);
}