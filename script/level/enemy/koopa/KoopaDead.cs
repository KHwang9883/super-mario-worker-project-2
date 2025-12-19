using Godot;
using System;
using SMWP;
using SMWP.Level;
using SMWP.Level.Sound;

public partial class KoopaDead : Node2D {
    [Signal]
    public delegate void PlaySoundLevelPassEventHandler();
    [Signal]
    public delegate void PlaySoundFasterLevelPassEventHandler();
    
    [Export] private ContinuousAudioStream2D _fallSound = null!;
    
    [Export] private PackedScene _smokeScene = GD.Load<PackedScene>("uid://c707h2fhiiirw");
    
    private int _timer;
    private float _speedY;
    private LevelConfig? _levelConfig;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
        
        // 击败库巴时暂停游戏
        GetTree().Paused = true;
        GameManager.TimeCountPause = true;
    }

    public override void _PhysicsProcess(double delta) {
        _timer++;
        if (_timer >= 50) {
            if (_levelConfig == null) {
                GD.PushError($"KoopaDead: LevelConfig is null!");
                return;
            }
            if (_levelConfig.FasterLevelPass) {
                if (!GameManager.IsLevelPass) {
                    var smoke = _smokeScene.Instantiate<Node2D>();
                    smoke.Position = Position;
                    AddSibling(smoke);
                    GameManager.IsLevelPass = true;
                    EmitSignal(SignalName.PlaySoundFasterLevelPass);
                    QueueFree();
                }
                return;
            }
        }
        if (_timer == 120) {
            _fallSound.Play();
        }
        if (_timer > 120) {
            Position += new Vector2(0f, _speedY);
            _speedY += 0.1f;
        }
        if (_timer == 200) {
            GameManager.IsLevelPass = true;
            EmitSignal(SignalName.PlaySoundLevelPass);
        }
    }
}
