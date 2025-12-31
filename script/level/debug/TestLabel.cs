using Godot;
using SMWP.Level.Player;

namespace SMWP.Level.Debug;

public partial class TestLabel : Label {
    [Export] private CharacterBody2D _player = null!;

    private PlayerMovement _playerMovement = null!;

    public override void _Ready() {
        _playerMovement = _player.GetNode<Node>("PlayerMediator").GetNode<PlayerMovement>("PlayerMovement");
    }

    public override void _PhysicsProcess(double delta) {
        Print();
    }

    public void Print() {
        if ((_player.ProcessMode == ProcessModeEnum.Disabled) || (_player == null)) {
            return;
        }
        Text = $"Position: ({_player.Position.X,9:00000.00}, {_player.Position.Y,9:00000.00})\n" +
               $"Velocity: ({_player.Velocity.X,7:000.00}, {_player.Velocity.Y,7:000.00})\n" +
               $"Speed: ({_playerMovement.SpeedX,7:000.00}, {_playerMovement.SpeedY,7:000.00})\n" +
               $"IsOnFloor: {_player.IsOnFloor()}\n" +
               $"IsOnCeiling: {_player.IsOnCeiling()}\n" +
               $"IsOnWall: {_player.IsOnWall()}\n" +
               $"InWater: {_playerMovement.IsInWater}\n" +
               $"OnWaterSurface: {_playerMovement.IsOnWaterSurface}\n" +
               $"Stuck: {_playerMovement.Stuck}\n\n" +
               $"PMeterCounter: {_playerMovement.PMeterCounter}\n" +
               $"MaxPMeter: {_playerMovement.MaxPMeter}"
               ;
    }
}
