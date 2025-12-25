using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using SMWP.Level;

public partial class BlockTileLayer : Node {
    [Export] private SmwlLevel? _smwlLevel;
    [Export] private Node2D _blockTile = null!;
    [Export] private GDC.Dictionary<LevelConfig.LayerOrderEnum, int> _targetZIndex = null!;
    
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        if (_smwlLevel != null) {
            _smwlLevel.LevelLoaded += SetZIndex;
        } else {
            SetZIndex();
        }
    }

    public void SetZIndex() {
        Callable.From(() => {
            _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
            
            _blockTile.ZIndex = _levelConfig.LayerOrder switch {
                LevelConfig.LayerOrderEnum.Classic =>
                    _targetZIndex.GetValueOrDefault(LevelConfig.LayerOrderEnum.Classic, _blockTile.ZIndex),
                
                LevelConfig.LayerOrderEnum.WaterAbove =>
                    _targetZIndex.GetValueOrDefault(LevelConfig.LayerOrderEnum.WaterAbove, _blockTile.ZIndex),
                
                LevelConfig.LayerOrderEnum.Modified =>
                    _targetZIndex.GetValueOrDefault(LevelConfig.LayerOrderEnum.Modified, _blockTile.ZIndex),
                
                _ => throw new ArgumentOutOfRangeException(),
            };
        }).CallDeferred();
    }
}
