using Godot;
using System;

public partial class PlatformSolid : StaticBody2D {
    [Export] private Area2D _area2D = null!;
    [Export] private PackedScene _perpetualMotionMachineMarkerScene = GD.Load<PackedScene>("uid://cvcg3dkclrihf");
    
    private bool _activate;
    private bool _detected;
    
    public override void _Ready() {
        Callable.From(() => {
            // SMWP2 关卡不支持 SMWP1 永动机
            var levelConfig = LevelConfigAccess.GetLevelConfig(this);
            if (levelConfig.SmwpVersion >= 2000) {
                return;
            }
            _activate = true;
        }).CallDeferred();
    }

    public override void _PhysicsProcess(double delta) {
        if (!_activate || _detected) {
            return;
        }
        var bodies = _area2D.GetOverlappingBodies();
        foreach (var body in bodies) {
            if (body is not PlatformSolid platformSolid) {
                continue;
            }
            if (platformSolid == this) {
                continue;
            }
            var perpetualMotionMachineMarker = _perpetualMotionMachineMarkerScene.Instantiate<Node2D>();
            AddChild(perpetualMotionMachineMarker);
            perpetualMotionMachineMarker.Position = new Vector2(16f, 16f);
            _detected = true;
            break;
        }
    }

    public void Detected() {
        _detected = true;
    }
}
