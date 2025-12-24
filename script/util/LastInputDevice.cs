using Godot;

namespace SMWP.Util;

public partial class LastInputDevice : Node {
    private static bool _isMouseInput;

    public override void _Input(InputEvent @event) {
        _isMouseInput = @event is InputEventMouse;
    }

    public static bool IsMouseLastInputDevice() {
        return _isMouseInput;
    }
}