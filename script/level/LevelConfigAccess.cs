using Godot;
using System;
using SMWP.Level;

public static class LevelConfigAccess {
    public const string LevelConfigGroup = "level_config";
    
    public static LevelConfig GetLevelConfig(Node contextNode) {
        return contextNode.GetTree().GetFirstNodeInGroup(LevelConfigGroup) as LevelConfig ?? throw new InvalidOperationException();
    }
    /*public static bool TryGetLevelConfig(Node contextNode, out LevelConfig? config) {
        config = GetLevelConfig(contextNode);
        return config != null;
    }*/
}