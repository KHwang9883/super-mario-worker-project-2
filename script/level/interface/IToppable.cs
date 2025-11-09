using Godot;

namespace SMWP.Level.Interface;

public interface IToppable {
    bool IsToppable { get; set; }
    
    public void MetadataInject(Node2D parent);
    public void OnTopped();
}