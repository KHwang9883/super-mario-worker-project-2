using Godot;

namespace SMWP.Level.Interface;

public interface IStarHittable {
    bool IsStarHittable { get; set; }
    
    public void MetadataInject(Node2D parent);
    public void OnStarmanHit(int score);
}