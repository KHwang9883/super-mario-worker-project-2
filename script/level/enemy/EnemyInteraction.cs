using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class EnemyInteraction : Node, IStompable, IHurtableAndKillable, IFireballHittable, IBeetrootHittable, IStarHittable, IShellHittable, IBumpHittable {
    [Signal]
    public delegate void StompedEventHandler();
    [Signal]
    public delegate void FireballHitEventHandler();
    [Signal]
    public delegate void BeetrootHitEventHandler();
    [Signal]
    public delegate void BumpedEventHandler();
    [Signal]
    public delegate void StarmanHitEventHandler();
    [Signal]
    public delegate void ShellHitEventHandler();
    [Signal]
    public delegate void NormalHitAddScoreEventHandler();
    [Signal]
    public delegate void ComboAddScoreEventHandler(int score);
    [Signal]
    public delegate void DiedEventHandler();
    [Signal]
    public delegate void PlaySoundStompedEventHandler();
    [Signal]
    public delegate void PlaySoundKickedEventHandler();
    [Signal]
    public delegate void PlaySoundBumpedEventHandler();
    
    [ExportGroup("Stomp")]
    [Export] public bool Stompable { get; set; } = true;
    [Export] public bool ImmuneToStomp { get; set; }
    [Export] public float StompOffset { get; set; } = -12f;
    [Export] public float StompSpeedY { get; set; } = -8f;
    
    [ExportGroup("HurtType")]
    [Export] public IHurtableAndKillable.HurtEnum HurtType { get; set; }
    
    [ExportGroup("Fireball")]
    [Export] public bool IsFireballHittable { get; set; } = true;
    [Export] public bool ImmuneToFireball { get; set; }
    [Export] public bool FireballExplode { get; set; } = true;
    
    [ExportGroup("Beetroot")]
    [Export] public bool IsBeetrootHittable { get; set; } = true;
    [Export] public bool ImmuneToBeetroot { get; set; }
    [Export] public bool BeetrootBump { get; set; }
    
    [ExportGroup("Star")]
    [Export] public bool IsStarHittable { get; set; } = true;
    [Export] public bool ImmuneToStar { get; set; }
    
    [ExportGroup("Shell")]
    [Export] public bool IsShellHittable { get; set; } = true;
    [Export] public int HardLevel { get; set; }
    [Export] public bool KillShell { get; set; }

    [ExportGroup("Bump")]
    [Export] public bool IsBumpHittable { get; set; } = true;
    [Export] public bool ImmuneToBump { get; set; }
    private Node2D? _parent;

    public override void _Ready() {
        _parent = (Node2D)GetParent();
        if (this is IStompable stompable) {
            stompable.MetadataInject(_parent);
        }
        if (this is IHurtableAndKillable hurtableAndKillable) {
            hurtableAndKillable.MetadataInject(_parent);
        }
        if (this is IFireballHittable fireballHittable) {
            fireballHittable.MetadataInject(_parent);
        }
        if (this is IBeetrootHittable beetrootHittable) {
            beetrootHittable.MetadataInject(_parent);
        }
        if (this is IStarHittable starHittable) {
            starHittable.MetadataInject(_parent);
        }
        if (this is IShellHittable shellHittable) {
            shellHittable.MetadataInject(_parent);
        }
        if (this is IBumpHittable bumpHittable) {
            bumpHittable.MetadataInject(_parent);
        }
    }
    
    // 玩家踩踏
    void IHurtableAndKillable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithHurt", this);
    }
    void IStompable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStomp", this);
    }
    public virtual float OnStomped(Node2D stomper) {
        // 这个方法被调用，说明已经是踩上了，判断语句在玩家的PlayerInteraction
        EmitSignal(SignalName.Stomped);
        if (ImmuneToStomp) {
            EmitSignal(SignalName.PlaySoundBumped);
            return StompSpeedY;
        }
        EmitSignal(SignalName.PlaySoundStomped);
        OnNormalAddScore();
        OnDied();
        return StompSpeedY;
    }
    public void PlayerHurtCheck(bool check) {
    }

    // 火球
    void IFireballHittable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithFireball", this);
    }
    public bool OnFireballHit(Node2D fireball) {
        EmitSignal(SignalName.FireballHit);
        if (!IsFireballHittable) return FireballExplode;
        if (ImmuneToFireball) {
            //EmitSignal(SignalName.PlaySoundBumped);
            return FireballExplode;
        }
        EmitSignal(SignalName.PlaySoundKicked);
        OnNormalAddScore();
        OnDied();
        return FireballExplode;
    }
    // 甜菜
    void IBeetrootHittable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithBeetroot", this);
    }
    public bool OnBeetrootHit(Node2D beetroot) {
        EmitSignal(SignalName.BeetrootHit);
        if (!IsBeetrootHittable) return BeetrootBump;
        if (ImmuneToBeetroot) {
            //EmitSignal(SignalName.PlaySoundBumped);
            return BeetrootBump;
        }
        EmitSignal(SignalName.PlaySoundKicked);
        OnNormalAddScore();
        OnDied();
        return BeetrootBump;
    }
    // 无敌星
    void IStarHittable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStar", this);
    }
    public bool OnStarmanHit(int score) {
        if (!IsStarHittable) return false;
        EmitSignal(SignalName.StarmanHit);
        OnComboAddScore(score);
        if (ImmuneToStar) return false;
        //EmitSignal(SignalName.PlaySoundKicked);
        OnDied();
        return true;
    }
    // 龟壳
    void IShellHittable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithShell", this);
    }
    public bool OnShellHit(int score) {
        if (!IsShellHittable) return false;
        EmitSignal(SignalName.ShellHit);
        OnComboAddScore(score);
        OnDied();
        return KillShell;
    }
    // 被顶
    void IBumpHittable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithBump", this);
    }
    public void OnBumped() {
        if (!IsBumpHittable) return;
        EmitSignal(SignalName.Bumped);
        if (ImmuneToBump) {
            EmitSignal(SignalName.PlaySoundBumped);
            return;
        }
        EmitSignal(SignalName.PlaySoundKicked);
        OnNormalAddScore();
        OnDied();
    }
    
    // 死亡
    public void OnDied() {
        EmitSignal(SignalName.Died);
    }
    // 普通死亡加分
    public void OnNormalAddScore() {
        EmitSignal(SignalName.NormalHitAddScore);
    }
    // 连击对象加分
    public void OnComboAddScore(int score) {
        EmitSignal(SignalName.ComboAddScore, score);
    }
}
