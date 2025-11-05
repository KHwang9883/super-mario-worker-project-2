using Godot;

namespace SMWP.Level.Interface;

public interface IBlockHittable {
    public void OnBlockHit(Node2D collider);
}