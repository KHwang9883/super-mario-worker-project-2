using Godot;

namespace SMWP.Level.Interface;

public interface IBeetrootHittable {
    bool IsBeetrootHittable { get; set; }
    bool BeetrootBump { get; set; }
    
    public void MetadataInject(Node2D parent);
    public bool OnBeetrootHit(Node2D beetroot);
}