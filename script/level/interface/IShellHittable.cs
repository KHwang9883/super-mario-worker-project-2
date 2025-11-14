using Godot;

namespace SMWP.Level.Interface;

public interface IShellHittable {
    bool IsShellHittable { get; set; }
    int HardLevel { get; set; }
    bool KillShell { get; set; }
    
    public void MetadataInject(Node2D parent);
    public bool OnShellHit(int score);
}