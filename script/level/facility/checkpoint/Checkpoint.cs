using Godot;
using System;
using SMWP;
using SMWP.Level;

public partial class Checkpoint : Area2D {
    [Signal]
    public delegate void CheckpointActivatedEventHandler();
    
    [Export] public int Id;
    [Export] public bool Activated;

    private LevelConfig? _levelConfig;
    private Water? _water;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        _water ??= (Water)GetTree().GetFirstNodeInGroup("water_global");
        
        // Activate activated
        if (!GameManager.ActivatedCheckpoints.Contains(Id)) return;
        Activated = true;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "activated";
    }
    public void OnBodyEntered(Node body) {
        Activate();
    }
    public bool Activate() {
        if (Activated) return false;
        Activated = true;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "activated";
        GameManager.CurrentCheckpointId = Id;
        GameManager.ActivatedCheckpoints.Add(Id);
        
        // 非自动流体运动下激活 Checkpoint 记录流体高度
        if (_water == null) {
            GD.PushError($"{this}: _water is null!");
        } else {
            // 注意要检测 LevelConfig 的 AutoFluid，而不是 Water 的
            if (_levelConfig == null) {
                GD.PushError($"{this}: LevelConfig is null!");
            } else {
                if (!_levelConfig.AutoFluid) {
                    GameManager.IsCheckpointWaterHeightRecorded = true;
                    GameManager.CheckpointWaterHeight = _water.Position.Y;
                    //GD.Print($"CheckpointWaterHeight: {GameManager.CheckpointWaterHeight}");
                }
                GameManager.CurrentBgmId = _levelConfig.BgmId;
                GameManager.CheckpointBgpId = _levelConfig.BgpId;
                GameManager.CheckpointRainyLevel = _levelConfig.RainyLevel;
                GameManager.CheckpointFallingStarsLevel = _levelConfig.FallingStarsLevel;
                GameManager.CheckpointSnowyLevel = _levelConfig.SnowyLevel;
                GameManager.CheckpointThunderLevel = _levelConfig.ThunderLevel;
                GameManager.CheckpointWindyLevel = _levelConfig.WindyLevel;
                GameManager.CheckpointDarkness = _levelConfig.Darkness;
                GameManager.CheckpointBrightness = _levelConfig.Brightness;
            }
        }
        
        EmitSignal(SignalName.CheckpointActivated);
        return true;
    }
}
