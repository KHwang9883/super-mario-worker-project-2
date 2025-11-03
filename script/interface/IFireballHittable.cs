using Godot;

namespace SMWP.Interface;

public interface IFireballHittable {
    public void OnFireballHit(Node2D fireball);
}