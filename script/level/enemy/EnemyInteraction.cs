using Godot;
using System;
using SMWP.Level.Interface;

namespace SMWP.Level.Enemy;

[GlobalClass]
public partial class EnemyInteraction : Node, IStompable, IHurtableAndKillable, IFireballHittable, IBeetrootHittable, IStarHittable, IToppable {
    [Signal]
    public delegate void StompedEventHandler();
    [Signal]
    public delegate void FireballHitEventHandler();
    [Signal]
    public delegate void BeetrootHitEventHandler();
    [Signal]
    public delegate void ToppedEventHandler();
    [Signal]
    public delegate void StarmanHitEventHandler();
    [Signal]
    public delegate void NormalHitAddScoreEventHandler();
    [Signal]
    public delegate void StarmanHitAddScoreEventHandler(int score);
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
    
    [ExportGroup("Bump")]
    [Export] public bool IsToppable { get; set; } = true;
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
        if (this is IToppable toppable) {
            toppable.MetadataInject(_parent);
        }
    }
    // 玩家踩踏
    void IHurtableAndKillable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithHurt", this);
    }
    void IStompable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithStomp", this);
    }
    public float OnStomped(Node2D stomper) {
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
    public void OnStarmanHit(int score) {
        EmitSignal(SignalName.StarmanHit);
        EmitSignal(SignalName.StarmanHitAddScore, score);
        if (ImmuneToStar) return;
        //EmitSignal(SignalName.PlaySoundKicked);
        OnDied();
    }
    // 被顶
    void IToppable.MetadataInject(Node2D parent) {
        parent?.SetMeta("InteractionWithBump", this);
    }
    public void OnTopped() {
        EmitSignal(SignalName.Topped);
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
}
