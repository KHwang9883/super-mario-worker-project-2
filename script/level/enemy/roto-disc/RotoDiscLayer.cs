using Godot;
using System;
using System.Collections.Generic;
using SMWP.Level;

public partial class RotoDiscLayer : Node {
    [Export] private Node2D _rotoDisc = null!;
    [Export] private int _extraZIndex = 2;
    
    [Export] public bool IsRotoDiscCoreScenery { get; private set; }
    
    private LevelConfig? _levelConfig;
    private BlockTileLayer _blockTileLayer = null!;

    public override void _Ready() {
        Callable.From(() => {
            _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
            _blockTileLayer = (BlockTileLayer)GetTree().GetFirstNodeInGroup("block_tile_layer");
            
            if (_levelConfig.RotoDiscLayer || IsRotoDiscCoreScenery) {
                _rotoDisc.ZIndex =
                    _blockTileLayer.TargetZIndex.GetValueOrDefault(
                        _levelConfig.LayerOrder, _rotoDisc.ZIndex) + _extraZIndex; 
            }
        }).CallDeferred();
    }
}
