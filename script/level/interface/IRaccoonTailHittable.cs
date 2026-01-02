using Godot;

namespace SMWP.Level.Interface;

public interface IRaccoonTailHittable {
    bool IsRaccoonTailHittable { get; set; }
    bool ImmuneToTail { get; set; }

    public void MetadataInject(Node2D parent);
    public bool OnRaccoonTailHit(Node2D tail);
}