using System.Collections.Generic;
using Godot;

namespace SMWP.Level.Score;

[GlobalClass]
public partial class AddScoreComponent : Node {
    /*public enum AddScoreEnum {
        Default,
        External,
    }

    [Export] public AddScoreEnum AddScoreType { get; set; }*/
    [Export] private bool _showCorrespondingScore = true;
    [Export] public int InternalScore = 200;

    private Node2D? _parent;
    private readonly Dictionary<int, Texture2D> _scoreTextures = new Dictionary<int, Texture2D>();
    private static readonly PackedScene ScoreEffectScene = GD.Load<PackedScene>("uid://ujklq52ucxun"); // 分数特效
    private static readonly Texture2D Score100 = GD.Load<Texture2D>("uid://dc87dph0iom7o");
    private static readonly Texture2D Score200 = GD.Load<Texture2D>("uid://uqh27twww0nn");
    private static readonly Texture2D Score500 = GD.Load<Texture2D>("uid://cqo76nqq4he0u");
    private static readonly Texture2D Score1000 = GD.Load<Texture2D>("uid://cqnxhjed803xi");
    private static readonly Texture2D Score2000 = GD.Load<Texture2D>("uid://b4emy2iet40l4");
    private static readonly Texture2D Score5000 = GD.Load<Texture2D>("uid://b1ys7srhy4oal");
    private static readonly Texture2D Score10000 = GD.Load<Texture2D>("uid://d1lohjqhcii88");
    private static readonly Texture2D Score1UP = GD.Load<Texture2D>("uid://bsni18sd05kvx");

    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        
        _scoreTextures[100] = Score100;
        _scoreTextures[200] = Score200;
        _scoreTextures[500] = Score500;
        _scoreTextures[1000] = Score1000;
        _scoreTextures[2000] = Score2000;
        _scoreTextures[5000] = Score5000;
        _scoreTextures[10000] = Score10000;
        _scoreTextures[-1] = Score1UP; // 1UP 作特殊处理，占用 -1 分
    }
    // 默认加分方法
    public void AddScore() {
        AddScoreToScoreManager(InternalScore);
        ShowCorrespondingScore(InternalScore);
    }
    // 无敌星、龟壳加分方法
    public void AddScore(int externalScore) {
        AddScoreToScoreManager(externalScore);
        ShowCorrespondingScore(externalScore);
    }
    public void AddScoreToScoreManager(int score) {
        LevelManager.AddScore(score);
    }
    public void ShowCorrespondingScore(int shownScore) {
        if (!_showCorrespondingScore) return;
        var scoreEffect = ScoreEffectScene.Instantiate<Sprite2D>();
        scoreEffect.SetTexture(_scoreTextures[shownScore]);
        if (_parent == null) return;
        scoreEffect.Position = _parent.Position;
        _parent.AddSibling(scoreEffect);
    }
}