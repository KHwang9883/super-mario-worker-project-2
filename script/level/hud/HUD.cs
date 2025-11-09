using Godot;
using System;
using SMWP.Level.Score;

namespace SMWP.Level.HUD;

public partial class HUD : Control {
    [Export] private Label? _life;
    [Export] private Label? _score;
    [Export] private Label? _levelTitle;
    [Export] private Label? _time;
    [Export] private Label? _coin;

    public override void _Process(double delta) {
        if (_score != null) _score.Text = ScoreManager.Score.ToString();
        
        // Todo: other hud items...
        
    }
}
