using Godot;
using System;
using SMWP.Level;
using SMWP.Level.Sound;

public partial class KoopaAttacked : Node {
    [Signal]
    public delegate void KoopaHurtEventHandler();
    [Signal]
    public delegate void KoopaDefeatedEventHandler();
    
    [Export] private PackedScene _koopaDeadScene = GD.Load<PackedScene>("uid://wfj3ukax5vf2");
    [Export] private AnimatedSprite2D _ani = null!;
    [Export] private ContinuousAudioStream2D _defeatedSound = null!;

    private LevelConfig? _levelConfig;
    
    // 全局共享 HP
    public static int KoopaEnergy;
    
    private bool _isHurtInvincible;
    private float _alphaValue;
    private bool _alphaTwist;

    public override void _Ready() {
        _levelConfig ??= LevelConfigAccess.GetLevelConfig(this);
    }
    public override void _PhysicsProcess(double delta) {
        if (_levelConfig == null) {
            GD.Print($"{this}: LevelConfig is null!");
            return;
        }
        KoopaEnergy = _levelConfig.KoopaEnergy;
        //GD.Print($"KoopaEnergy: {KoopaEnergy}");
        
        if (!_isHurtInvincible) return;

        if (Math.Abs(_alphaValue - 1f) < 0.01f) {
            _alphaTwist = true;
        }
        if (_alphaValue == 0f) {
            _alphaTwist = false;
        }
        _alphaValue = 
            !_alphaTwist
                ? Mathf.MoveToward(_alphaValue, 1f, 0.04f)
                : Mathf.MoveToward(_alphaValue, 0f, 0.03f);
        
        _ani.Modulate = _ani.Modulate with { A = _alphaValue };
    }

    public void OnAttacked() {
        if (KoopaEnergy <= 1) {
            KoopaEnergy = 0;
            _defeatedSound.Play();
            EmitSignal(SignalName.KoopaDefeated);
            CreateDead();
            GetParent().QueueFree();
        } else {
            KoopaEnergy -= 1;
        }
        
        // 再赋值回去
        _levelConfig!.KoopaEnergy = KoopaEnergy;
        
        _isHurtInvincible = true;
        EmitSignal(SignalName.KoopaHurt);
    }
    public void OnHurtInvincibleEnded() {
        _isHurtInvincible = false;
        _ani.Modulate = _ani.Modulate with { A = 1f };
        _alphaValue = 0f;
        _alphaTwist = false;
    }
    public void CreateDead() {
        var parent = GetParent<Node2D>();
        var koopaDead = _koopaDeadScene.Instantiate<Node2D>();
        koopaDead.Position = parent.Position;
        parent.AddSibling(koopaDead);
    }
}
