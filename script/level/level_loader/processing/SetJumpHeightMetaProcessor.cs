using Godot;
using SMWP.Level.Physics;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;
    
public partial class SetJumpHeightMetaProcessor : ObjectProcessor {
    public override void ProcessObject(Node instance, string metadata) {
        if (float.TryParse(metadata, out var jumpHeight)) {
            if (instance.TryGetComponent(out BasicMovement? movement)) {
                movement.JumpSpeed = -jumpHeight;
            }
        }
    }
}