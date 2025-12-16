using Godot;
using System;
using SMWP.Level.Sound;

public partial class KoopaAttacked : Node {
    [Signal]
    public delegate void KoopaHurtEventHandler();
    
    [Export] private PackedScene _koopaDeadScene = GD.Load<PackedScene>("uid://wfj3ukax5vf2");
    [Export] private AnimatedSprite2D _ani = null!;
    [Export] private ContinuousAudioStream2D _defeatedSound = null!;
    
    // 全局共享 HP
    public static int KoopaEnergy;
    // 自己的 HP（在全局 HP 更新后更新）
    private int _health;
    private bool _isHurtInvincible;
    private float _alphaValue;
    private bool _alphaTwist;

    public override void _Ready() {
        KoopaEnergy = LevelConfigAccess.GetLevelConfig(this).KoopaEnergy;
        _health = KoopaEnergy;
    }
    public override void _PhysicsProcess(double delta) {
        if (!_isHurtInvincible) return;

        if (Math.Abs(_alphaValue - 1f) < 0.04f) {
            _alphaTwist = true;
        }
        if (_alphaValue == 0f) {
            _alphaTwist = false;
        }
        if (!_alphaTwist) {
            Mathf.MoveToward(_alphaValue, 1f, 0.04f);
        } else {
            Mathf.MoveToward(_alphaValue, 0f, 0.03f);
        }
        GD.Print(_alphaValue);
        GD.Print(_alphaTwist);
        _ani.Modulate = _ani.Modulate with { A = _alphaValue };
    }

    public void OnAttacked() {
        if (KoopaEnergy == 1) {
            KoopaEnergy = 0;
            _defeatedSound.Play();
            CreateDead();
        } else {
            KoopaEnergy -= 1;
        }
        _health = KoopaEnergy;
        _isHurtInvincible = true;
        EmitSignal(SignalName.KoopaHurt);
    }
    public void OnHurtInvincibleEnded() {
        _isHurtInvincible = false;
        _ani.Modulate = _ani.Modulate with { A = 1f };
        _alphaValue = 0f;
    }
    public void CreateDead() {
        var parent = GetParent<Node2D>();
        var koopaDead = _koopaDeadScene.Instantiate<Node2D>();
        koopaDead.Position = parent.Position;
        parent.AddSibling(koopaDead);
    }
}
