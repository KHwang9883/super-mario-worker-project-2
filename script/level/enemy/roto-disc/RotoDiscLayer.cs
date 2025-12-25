using Godot;
using System;
using SMWP.Level;

public partial class RotoDiscLayer : Node {
    [Export] private Node2D _rotoDisc = null!;
    [Export] private int _targetZIndex = 400;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        Callable.From(() => {
            _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
            if (_levelConfig.RotoDiscLayer) {
                _rotoDisc.ZIndex = _targetZIndex;
            }
        }).CallDeferred();
    }
}
