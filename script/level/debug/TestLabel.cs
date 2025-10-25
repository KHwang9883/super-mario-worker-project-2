using Godot;

namespace SMWP.Level.Debug;

public partial class TestLabel : Label {
    [Export] private PMovement _player = null!;
    
    public override void _PhysicsProcess(double delta) {
        Print();
    }

    public void Print() {
        Text = $"Position: ({_player.Position.X,9:00000.00}, {_player.Position.Y,9:00000.00})\n" +
               $"Velocity: ({_player.Velocity.X,7:000.00}, {_player.Velocity.Y,7:000.00})\n" +
               $"IsOnFloor: {_player.IsOnFloor()}\n" +
               $"IsOnCeiling: {_player.IsOnCeiling()}\n" +
               $"IsOnWall: {_player.IsOnWall()}\n" +
               $"InWater: {_player.IsInWater}\n" +
               $"OnWaterSurface: {_player.IsOnWaterSurface}";
    }
}
