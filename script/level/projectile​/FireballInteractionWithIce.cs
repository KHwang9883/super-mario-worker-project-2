using Godot;
using System;
using SMWP.Level.Physics;

[GlobalClass]
public partial class FireballInteractionWithIce : Node {
    [Signal]
    public delegate void FireballExplodeEventHandler();

    [Export] private BasicMovement _basicMovement = null!;
    private CharacterBody2D _fireball = null!;

    public override void _Ready() {
        _fireball = GetParent<CharacterBody2D>();
        //_fireball.SetMeta("InteractionWithIce", this);
        //Overlap();
    }
    public override void _PhysicsProcess(double delta) {
        Overlap();
    }

    public void Overlap() {
        // 撞击冰块检测
        Node? interactionWithBlockNode = null;
        
        var collision = _fireball.MoveAndCollide(
            new Vector2(_basicMovement.SpeedX, _basicMovement.SpeedY), true, 0.06f
        );
        //GD.Print($"Fireball Collision: {collision}");
        var collider = collision?.GetCollider();
        if (!IsInstanceValid(collider)) return;
        if (collider is not StaticBody2D staticBody) return;
        if (staticBody.HasMeta("InteractionWithBlock")) {
            interactionWithBlockNode = (Node)staticBody.GetMeta("InteractionWithBlock");
        }
        if (interactionWithBlockNode is not IceBlock iceBlock) return;
        iceBlock.OnBlockHit(_fireball);
        
        EmitSignal(SignalName.FireballExplode);
    }
}
