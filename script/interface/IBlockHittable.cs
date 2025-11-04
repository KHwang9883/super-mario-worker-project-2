using Godot;

namespace SMWP.Interface;

public interface IBlockHittable {
    public void OnBlockHit(Node2D collider);
}