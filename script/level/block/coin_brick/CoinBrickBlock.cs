using Godot;
using System;
using SMWP.Level.Block;

public partial class CoinBrickBlock : BlockHit {
    [Export] public PackedScene SproutItemScene = GD.Load<PackedScene>("uid://bb7ac4wrewh71");
    [Export] private AnimatedSprite2D _ani = null!;
    private const int CoinBrickTime = 20;
    private const int CoinBrickHitCount = 15;
    private bool _onCoinBrickTimeCount;
    private int _coinBrickHitCounter;
    private int _coinBrickTimer;
    
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (!_onCoinBrickTimeCount) return;
        if (_coinBrickTimer < CoinBrickTime) {
            _coinBrickTimer++;
        } else {
            if (_coinBrickHitCounter >= CoinBrickHitCount) return;
            _coinBrickHitCounter++;
            _coinBrickTimer = 0;
        }
    }
    
    protected override void OnBlockBump() {
        base.OnBlockBump();
        _ani.Play("hit");
        OnSproutItem();
    }
    protected override void OnBumped() {
        base.OnBumped();
        _ani.Play("default");
        if (_coinBrickHitCounter < CoinBrickHitCount) return;
        Bumpable = false;
        _ani.Play("unhittable");
    }
    public void OnSproutItem() {
        _onCoinBrickTimeCount = true;
        var sproutItem = SproutItemScene.Instantiate<Node2D>();
        sproutItem.Position = Parent.Position;
        Parent.AddSibling(sproutItem);
    }
}
