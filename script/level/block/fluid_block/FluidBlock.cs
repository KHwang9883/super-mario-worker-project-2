using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Block;

public partial class FluidBlock : BlockHit {
    [Export] public float TargetHeight;
    [Export] public float Speed = 1f;

    private LevelConfig? _levelConfig;
    private Water? _water;
    private AnimatedSprite2D? _ani;

    private SpriteFrames _waterSpriteFrames = GD.Load<SpriteFrames>("uid://7cvg5krxkrkh");
    private SpriteFrames _lavaSpriteFrames = GD.Load<SpriteFrames>("uid://cn842nqqv576b");
    
    public override void _Ready() {
        base._Ready();
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _water ??= (Water)GetTree().GetFirstNodeInGroup("water_global");
        if (Sprite is AnimatedSprite2D ani) {
            _ani = ani;
        }
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (_levelConfig == null) {
            GD.PushError($"{this}: LevelConfig is null!");
            return;
        }
        
        switch (_levelConfig.FluidType) {
            case Fluid.FluidTypeEnum.Water:
                _ani?.SetSpriteFrames(_waterSpriteFrames);
                break;
            case Fluid.FluidTypeEnum.Lava:
                _ani?.SetSpriteFrames(_lavaSpriteFrames);
                break;
        }
    }

    protected override void OnBlockBump() {
        if (_water == null) {
            GD.PushError($"{this}: _water is null!");
            return;
        }
        if (Math.Abs(_water.GetTargetHeight() - TargetHeight) < 0.08f) return;
        
        base.OnBlockBump();
        
        _water.FluidControlSet(TargetHeight, Speed);
        _ani?.Play("hit");
    }
    protected override void OnBumped() {
        _ani?.Play("default");
        Bumpable = true;
        base.OnBumped();
        if (!Bumpable) _ani?.Play("disabled");
    }
}
