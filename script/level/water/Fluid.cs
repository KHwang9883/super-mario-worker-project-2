using Godot;
using System;
using SMWP.Level;

public partial class Fluid : Node2D {
    public enum FluidTypeEnum { Water, Lava }
    [Export] public FluidTypeEnum FluidType = FluidTypeEnum.Water;

    [Export] private Area2D _water = null!;
    [Export] private Area2D _lava = null!;

    private LevelConfig? _levelConfig;
    
    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.PushError($"{this}: Level Config is null!");
        } else {
            FluidType = _levelConfig.FluidType;
        }
    }
}
