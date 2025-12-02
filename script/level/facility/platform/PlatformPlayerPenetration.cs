using Godot;
using System;
using SMWP.Level.Player;

public partial class PlatformPlayerPenetration : Node {
    private StaticBody2D? _parent;
    private Node? _player;
    private PlayerMovement? _playerMovement;
    private uint _collisionLayer;
    private bool _isPlayerCurrentStep;

    public override void _Ready() {
        _parent ??= GetParent<StaticBody2D>();
        _collisionLayer = _parent.GetCollisionLayer();
        
        _player ??= GetTree().GetFirstNodeInGroup("player");
        if (_player.HasMeta("PlayerMovement")) {
            _playerMovement = (PlayerMovement)_player.GetMeta("PlayerMovement");
        }
    }
    public override void _PhysicsProcess(double delta) {
        if (_playerMovement == null || _parent == null) return;
        
        // 玩家在垂直移动平台上时，允许穿透其他平台
        _parent.SetCollisionLayer(
            _playerMovement.OnVerticalPlatform ? (uint)0 : _collisionLayer
            );
        
        if (_isPlayerCurrentStep) _parent.SetCollisionLayer(_collisionLayer);

        _isPlayerCurrentStep = false;
    }
    public void OnPlayerCurrentStep() {
        _isPlayerCurrentStep = true;
    }
}
