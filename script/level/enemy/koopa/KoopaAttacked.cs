using Godot;
using System;

public partial class KoopaAttacked : Node {
    [Export] private PackedScene _koopaDeadScene = GD.Load<PackedScene>("uid://wfj3ukax5vf2");
    
    public void OnAttacked() {
        // Todo: Hp -= 1
        
        // Todo: Hp == 0 -> 油碟
    }

    public void CreateDead() {
        var parent = GetParent<Node2D>();
        var koopaDead = _koopaDeadScene.Instantiate<Node2D>();
        koopaDead.Position = parent.Position;
        parent.AddSibling(koopaDead);
    }
}
