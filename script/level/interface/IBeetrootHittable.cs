using Godot;

namespace SMWP.Level.Interface;

public interface IBeetrootHittable {
    bool BeetrootBump { get; set; }
    
    public bool OnBeetrootHit(Node2D beetroot);
}