using Godot;

namespace SMWP.Level.Interface;

public interface IBumpHittable {
    bool IsBumpHittable { get; set; }
    
    public void MetadataInject(Node2D parent);
    public void OnBumped();
}