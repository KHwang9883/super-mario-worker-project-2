using Godot;
using System;

namespace SMWP.Level.Bonus.Lui;

public partial class LuiMovement : Node {
    [Export] private BonusSprout _bonusSprout = null!;
    
    [Export] private Area2D _lui = null!;
    private float _originY;
    private bool _originYSet;
    private float _speedY = -8f;
    
    public override void _PhysicsProcess(double delta) {
        if (!_bonusSprout.Overlapping && !_originYSet) {
            _originY = _lui.Position.Y;
            _originYSet = true;
        }
        
        if (!_originYSet && _bonusSprout.Overlapping) return;
        
        _lui.Position = new Vector2(_lui.Position.X, _lui.Position.Y + _speedY);
        _speedY += 0.4f;
        if (_speedY > 8f) {
            _lui.Position = new Vector2(_lui.Position.X, _originY);
            _speedY = -8f;
        }
    }
}
