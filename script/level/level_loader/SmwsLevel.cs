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
        
        // 禁止 God Mode
        IsGodMode = false;

        _gameManager ??= GetTree().Root.GetNode<GameManager>("/root/GameManager");
        _gameManager.ScenarioNextLevel += NextLevel;
        
        if (ScenarioFile == null) {
            // 测试用
            // 改成拖拽加载了，更方便（
            GetWindow().FilesDropped += files => OnOpenSmwlDialogFileSelected(files[0]);
        } else {
            // 加载Scenario数据
            LoadScenario();
        }
    }
    
    private async void LoadScenario() {
        if (ScenarioFile == null || !File.Exists(ScenarioFile)) {
            return;
        }
        
        var smwlLoader = new SmwlLoader();
        await using var input = File.OpenRead(ScenarioFile);
        
        if (await SmwsLoader.Load(input, smwlLoader) is {} scenario) {
            _scenario = scenario;
            // 设置GameManager的Scenario相关变量
            IsPlayingScenario = true;
            ScenarioLevelCount = scenario.Levels.Count;
            // 加载当前关卡，而不是下一个关卡
            _currentLevel = CurrentScenarioLevel;
            if (_currentLevel < _scenario.Levels.Count) {
                NextLevel(_scenario.Levels[_currentLevel]);
            }
        } else {
            foreach (var error in SmwsLoader.ErrorMessage) {
                GD.PrintErr(error);
            }
        }
        
        smwlLoader.QueueFree();
    }

    private async void OnOpenSmwlDialogFileSelected(string file) {
        if (File.Exists(file)) {
            ScenarioFile = file;
            
            var smwlLoader = new SmwlLoader();
            await using var input = File.OpenRead(file);
            
            if (await SmwsLoader.Load(input, smwlLoader) is {} scenario) {
                _scenario = scenario;
                // 设置GameManager的Scenario相关变量
                IsPlayingScenario = true;
                ScenarioLevelCount = scenario.Levels.Count;
                CurrentScenarioLevel = 0;
                if (scenario.Levels.Count > 0) {
                    NextLevel(scenario.Levels[0]);   
                }
            } else {
                foreach (var error in SmwsLoader.ErrorMessage) {
                    GD.PrintErr(error);
                }
            }
            
            smwlLoader.QueueFree();
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
            // Scenario 全部完成，此时GameManager会处理返回标题界面和重置数据
            return;
        }
        _currentLevel++;
        NextLevel(_scenario.Levels[_currentLevel]);
    }
    
    public void NextLevel(SmwlLevelData levelData) {
        if (_smwlLevel != null && IsInstanceValid(_smwlLevel)) {
            _smwlLevel.Free();
        }
        
        _smwlLevel = _smwlLevelScene.Instantiate<SmwlLevel>();
        ScenarioNewLevelLineNum.TryGetValue(CurrentScenarioLevel, out var lineNum);
        //GD.Print($"lineNum: {lineNum}");
        _smwlLevel.SmwlLoader.LineNum = lineNum;
        _smwlLevel.Install(levelData);
        AddChild(_smwlLevel);
    }
    
    public void JumpToScene(string sceneUid) {
        IsPlayingScenario = false;
        GetTree().ChangeSceneToFile(sceneUid);
    }
}
