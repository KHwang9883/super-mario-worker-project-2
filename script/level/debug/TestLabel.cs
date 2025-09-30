using Godot;

namespace SMWP.Level.Debug;

public partial class TestLabel : Label {
    [Export] private CharacterBody2D _mario = null!;
    
    public override void _Process(double delta) {
        Text = $"Position: ({_mario.Position.X,9:00000.00}, {_mario.Position.Y,9:00000.00})\n" +
               $"Velocity: ({_mario.Velocity.X,7:000.00}, {_mario.Velocity.Y,7:000.00})";
    }
}
