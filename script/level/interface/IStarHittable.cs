using Godot;

namespace SMWP.Level.Interface;

public interface IStarHittable {
    bool IsStarHittable { get; set; }
    bool ImmuneToStar { get; set; }
    
    public void MetadataInject(Node2D parent);
    public bool OnStarmanHit(int score);
}