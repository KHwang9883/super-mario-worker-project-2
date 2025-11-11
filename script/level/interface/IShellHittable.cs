using Godot;

namespace SMWP.Level.Interface;

public interface IShellHittable {
    bool IsShellHittable { get; set; }
    bool HardToShell { get; set; }
    
    public void MetadataInject(Node2D parent);
    public bool OnShellHit(int score);
}