using Godot;

namespace SMWP.Level.Interface;

public interface IBlockHittable {
    public void MetadataInject(Node2D parent);
    public void OnBlockHit(Node2D collider);
}