using Godot;
using System;
using SMWP.Level.Block;

public partial class IceBlock : BlockHit {
    [Signal]
    public delegate void PlaySoundIceBlockHitEventHandler();
    [Signal]
    public delegate void PlaySoundIceBlockBreakEventHandler();
    
    private int _iceHp = 2;
    private AnimatedSprite2D? _ani;
    private bool _breakable;

    public override void _Ready() {
        base._Ready();
        if (Sprite is AnimatedSprite2D ani) {
            _ani = ani;
        }
    }
    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (_ani == null) {
            GD.PushError($"{this}: _ani is null!");
            return;
        }
        
        switch (_iceHp) {
            case 2:
                _ani.Play("default");
                break;
            case 1:
                _ani.Play("hit");
                break;
        }
    }

    protected override bool IsBreakable(Node2D collider) {
        OnIceBlockHit();
        if (_iceHp > 0) return false;
        
        _breakable = (collider.IsInGroup("hittable_to_ice")
            || collider.HasMeta("ThwompInteraction")
            );
        return _breakable;
    }
    public void OnIceBlockHit() {
        _iceHp--;
        switch (_iceHp) {
            case 1:
                EmitSignal(SignalName.PlaySoundIceBlockHit);
                break;
            case 0:
                EmitSignal(SignalName.PlaySoundIceBlockBreak);
                break;
        } 
    }
}
