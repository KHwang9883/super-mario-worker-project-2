using Godot;

namespace SMWP.Level.Interface;

public interface IFireballHittable {
    bool IsFireballHittable { get; set; }
    bool FireballExplode { get; set; }

    public void MetadataInject(Node2D parent);
    public bool OnFireballHit(Node2D fireball);
}