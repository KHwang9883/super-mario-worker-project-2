using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using FileAccess = Godot.FileAccess;

public partial class LevelSave : Node {
    [Signal] public delegate void LevelSavedEventHandler();
    
    [ExportGroup("Ref")]
    [Export] public FileDialog DialogWindow = null!;
    
    [ExportCategory("Compress Setting")]
    [Export] public bool CompressLevel = false;

    [ExportCategory("Ref")]
    [Export] public LevelSettings Settings { get; set; } = null!;
    [Export] public TileMapLayer BlocksTileMapLayer { get; set; } = null!;
    [Export] public Node2D ObjectNode2D { get; set; } = null!;

    public GDC.Dictionary LevelDict = new();
    
    public string CurrentPath = "user://Untitled Level.smwl";

    public override void _Ready() {
        base._Ready();
        // Godot 官方人员说 AcceptWindow 类的 Confirmed 信号不是给 FileDialog 用的
        // Confirmed 信号用在 Native Window 会 bug（点确认框后不发射信号），Godot 窗口则不会
        // 官方表示如果要用就必须用 FileDialog 的信号，比如 FileSelected
        // 然后他们在 pull request 中加了 Godot 4.8 要完善 Confirmed 信号相关文档的 pr
        // 何意味
        //DialogWindow.Confirmed += OnFileDialogConfirmed;
        //DialogWindow.Confirmed += SaveLevel;
        DialogWindow.FileSelected += OnSaveAFile;
    }

    public void SaveLevel() {
        // Clear cached level data
        LevelDict.Clear();
        
        // Save version
        LevelDict["version"] = Settings.SmwpVersion;
        
        // Save level settings
        SaveLevelSettings();
        
        // Save tilemap data
        LevelDict["blocks"] = BlocksTileMapLayer.TileMapData;
        
        // Save objects data
        SaveObjects();
        
        // To JSON
        string json = Json.Stringify(LevelDict, "\t");
        GD.Print("Level data: " + json);
        
        // Compress (Optional)
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] compressedBytes;
        if (CompressLevel) {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Fastest)) {
                gzip.Write(jsonBytes, 0, jsonBytes.Length);
            }
            compressedBytes = output.ToArray();
        } else {
            compressedBytes = jsonBytes;
        }
        
        // Save to disc
        string fullPath = CurrentPath;
        using var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Write);
        if (file == null) {
            GD.PushError($"无法打开文件保存: {fullPath}");
            return;
        }
        file.StoreBuffer(compressedBytes);
        GD.Print($"Level saved to: {fullPath}");

        EmitSignal(SignalName.LevelSaved);
    }

    public void SaveLevelSettings() {
        // TODO: Save level settings
    }

    public void SaveObjects() {
        var objArray = new GDC.Array();
        foreach (var editNode in ObjectNode2D.GetChildren()) {
            var spawnerObjectNode = editNode.GetNodeOrNull<SpawnerObject>("EditObjectBase/SpawnerObject");
            if (spawnerObjectNode == null) {
                GD.PushError($"{editNode.Name} is missing SpawnerObject!");
                continue;
            }
            
            var objectDict = new GDC.Dictionary();
            objectDict["id"] = spawnerObjectNode.SpawnerIdStr;
            // 这里保存 Marker2D 全局位置（和SMWP1偏移保持一致）
            objectDict["pos"] = spawnerObjectNode.GetNode<Marker2D>("../LeftTopMarker2D").GlobalPosition;
            if (spawnerObjectNode.MetaDict != null) {
                objectDict["meta"] = spawnerObjectNode.MetaDict;
            }
            objArray.Add(objectDict);
        }
        LevelDict["objects"] = objArray;
    }

    public void OnSaveAFile(string path = "") {
        CurrentPath = DialogWindow.CurrentPath;
        SaveLevel();
    }
    /*
    public void OnFileDialogFileSelected(string path) {
        SaveLevel();
        CurrentFile = DialogWindow.CurrentFile;
    }
    */
}