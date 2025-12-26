using Godot;
using System;
using System.IO;
using SMWP;
using SMWP.Level;
using static SMWP.GameManager;

public partial class SmwsLevel : Node2D {
    [Export] private PackedScene _smwlLevelScene = GD.Load<PackedScene>("uid://c5d467llk4uur");
    [Export] public SmwsLoader SmwsLoader = null!;

    private SmwlLevel? _smwlLevel;
    private GameManager? _gameManager;
    
    public override void _Ready() {
        base._Ready();

        _gameManager ??= GetTree().Root.GetNode<GameManager>("/root/GameManager");
        // 下一个关卡：删除当前关卡
        _gameManager.ScenarioNextLevel += NextLevel;
        
        if (LevelFileStream == null) {
            // 测试用
            // 改成拖拽加载了，更方便（
            GetWindow().FilesDropped += files => OnOpenSmwlDialogFileSelected(files[0]);
        } else {
            GD.PushError("LevelFileStream is not null!");
        }
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            await using var input = File.OpenRead(file);
            /*
            if (await SmwsLoader.Load(input) is { } data) {
                
                
                LevelFileStream = data;
                //Install(data);
            } else {
                foreach (var error in SmwsLoader.ErrorMessage) {
                    GD.PrintErr(error);
                }
            }
            */
        } else {
            GD.PrintErr($"File {file} does not exist");
        }
    }

    // Todo: 第一行：Lives读取
    // Todo: 第二行（可能没有这一行）：自定义 BGM 包名
    // Todo: 第三行：第一个NewLevel
    // Todo: NewLevel 行号存储
    
    public void LinesSet(int levelCount, int lineCount) {
        ScenarioNewLevelLineNum.Add(levelCount, lineCount);
    }
    // 下一个关卡：删除当前关卡
    public void NextLevel() {
        _smwlLevel?.QueueFree();
        
        _smwlLevel = _smwlLevelScene.Instantiate<SmwlLevel>();
        ScenarioNewLevelLineNum.TryGetValue(CurrentScenarioLevel, out var lineNum);
        _smwlLevel.SmwlLoader.LineNum = lineNum;
        AddChild(_smwlLevel);
    }
}
