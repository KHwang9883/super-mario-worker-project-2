using Godot;

namespace SMWP.Level.Interface;

public interface IFireballHittable {
    bool FireballExplode { get; set; }
    
    public bool OnFireballHit(Node2D fireball);
}