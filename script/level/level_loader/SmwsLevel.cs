using Godot;
using System.IO;
using SMWP;
using SMWP.Level;
using SMWP.Level.Data;
using static SMWP.GameManager;

public partial class SmwsLevel : Node2D {
    [Export] private PackedScene _smwlLevelScene = GD.Load<PackedScene>("uid://c5d467llk4uur");
    [Export] public SmwsLoader SmwsLoader = null!;

    private SmwsScenarioData? _scenario;
    private SmwlLevel? _smwlLevel;
    private GameManager? _gameManager;
    private int _currentLevel = 0;
    
    public override void _Ready() {
        base._Ready();

        _gameManager ??= GetTree().Root.GetNode<GameManager>("/root/GameManager");
        _gameManager.ScenarioNextLevel += NextLevel;
        
        if (ScenarioFile == null) {
            // 测试用
            // 改成拖拽加载了，更方便（
            GetWindow().FilesDropped += files => OnOpenSmwlDialogFileSelected(files[0]);
        } else {
            NextLevel();
        }
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            ScenarioFile = file;
            await using var input = File.OpenRead(file);
            
            if (await SmwsLoader.Load(input, _smwlLevel.SmwlLoader) is {} scenario) {
                _scenario = scenario;
                if (scenario.Levels.Count > 0) {
                    NextLevel(scenario.Levels[0]);   
                }
            } else {
                foreach (var error in SmwsLoader.ErrorMessage) {
                    GD.PrintErr(error);
                }
            }
        } else {
            GD.PrintErr($"File {file} does not exist");
        }
    }
    
    // 下一个关卡：删除当前关卡（由 GameManager 发射信号）
    public void NextLevel() {
        if (_scenario is not { } scenario) {
            GD.PushError($"No loaded scenario in {nameof(SmwsLevel)}");
            return;
        }
        if (_currentLevel + 1 >= scenario.Levels.Count) {
            // Scenario 全部完成，此时应返回标题界面
            return;
        }
        _currentLevel++;
        NextLevel(scenario.Levels[_currentLevel]);
    }
    
    public void NextLevel(SmwlLevelData levelData) {
        _smwlLevel?.QueueFree();
        
        _smwlLevel = _smwlLevelScene.Instantiate<SmwlLevel>();
        ScenarioNewLevelLineNum.TryGetValue(CurrentScenarioLevel, out var lineNum);
        GD.Print($"lineNum: {lineNum}");
        _smwlLevel.SmwlLoader.LineNum = lineNum;
        _smwlLevel.Install(levelData);
        AddChild(_smwlLevel);
    }
    
    public void JumpToScene(string sceneUid) {
        IsPlayingScenario = false;
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
