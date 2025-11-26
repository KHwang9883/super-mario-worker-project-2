using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Tool;

public partial class FallingStarsController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export] private PackedScene _starScene = GD.Load<PackedScene>("uid://qdl5o6r7cbjq");
    public int FallingStarsLevel;
    
    private LevelConfig? _levelConfig;
    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        _levelConfig = LevelConfigAccess.GetLevelConfig(this);
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) return;
        
        FallingStarsLevel = _levelConfig.FallingStarsLevel;
        switch (FallingStarsLevel) {
            case 0:
                break;
            case 1:
                GetNode<SmwpFrameTimer>("Level1Timer").Start();
                break;
            case 2:
                GetNode<SmwpFrameTimer>("Level2Timer").Start();
                break;
            case 3:
                Create();
                break;
        }
        //GD.Print("FallingStarsLevel: " + FallingStarsLevel);
        //GD.Print($"FallingStars Count: {GetTree().GetNodeCountInGroup("falling_star_pool")}");
    }
    public void Create() {
        var star = (GetTree().GetNodesInGroup("falling_star_pool").Count < 60)
            ? _starScene.Instantiate<FallingStar>()
            : (Node2D)GetTree().GetFirstNodeInGroup("falling_star_pool");
        star.Position =
            ScreenUtils.GetScreenRect(this).Position +
            new Vector2(-32f + _rng.RandfRange(0f, 960f), -32f);
        star.ResetPhysicsInterpolation();
        if (star.IsInGroup("falling_star_pool")) {
            if (star is not FallingStar starNode) return;
            starNode.Reset();
            star.RemoveFromGroup("falling_star_pool");
        } else {
            _weatherController.AddSibling(star);
        }
    }
}
