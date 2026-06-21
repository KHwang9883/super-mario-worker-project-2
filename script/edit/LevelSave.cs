using Godot;
using System;

public partial class LevelSave : Node {
    [ExportCategory("Compress Setting")]
    [Export] public bool CompressLevel = false;

    [ExportCategory("Ref")]
    [Export] public LevelSettings Settings { get; set; } = null!;
    [Export] public TileMapLayer BlocksTileMapLayer { get; set; } = null!;
    [Export] public Node2D ObjectNode2D { get; set; } = null!;

    public string CurrentFilePath = "user://";

    public void SaveLevel() {
        // TODO: Save Level

        var levelDict = new GDC.Dictionary();
        
        // Save version
        levelDict["version"] = Settings.SmwpVersion;
        
        // TODO: Save level settings
        
        // Save tilemap data
        levelDict["blocks"] = BlocksTileMapLayer.TileMapData;
        
        // Save objects data
        var objArray = new GDC.Array();
        foreach (var editNode in ObjectNode2D.GetChildren()) {
            var spawnerObjectNode = editNode.GetNodeOrNull<SpawnerObject>("EditObjectBase/SpawnerObject");
            if (spawnerObjectNode == null) {
                GD.PushError($"{editNode.Name} is missing SpawnerObject!");
                continue;
            }
            
            var objectDict = new GDC.Dictionary();
            objectDict["id"] = spawnerObjectNode.SpawnerIdStr;
            // TODO: 微调的时候改谁的位置？
            // 这里暂定 Marker2D 全局位置（和SMWP1偏移保持一致）
            objectDict["pos"] = spawnerObjectNode.GetNode<Marker2D>("../LeftTopMarker2D").GlobalPosition;
            objectDict["meta"] = spawnerObjectNode.MetaDict;
            objArray.Add(objectDict);
        }
        levelDict["objects"] = objArray;
        
        // To JSON
        string json = Json.Stringify(levelDict, "\t");
        
        // Compress (Optional)
        if (CompressLevel) {
            // TODO: Compress data
        }
        
        // TODO: Save to disc
    }
}
