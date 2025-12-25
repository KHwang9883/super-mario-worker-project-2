using Godot;
using System;
using SMWP.Level;

public partial class BlockTileLayer : Node {
    [Export] private Node2D _blockTile = null!;
    [Export] private int _targetZIndex = 350;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        Callable.From(() => {
            _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
            if (_levelConfig.RotoDiscLayer) {
                _blockTile.ZIndex = _targetZIndex;
            }
        }).CallDeferred();
    }
}
