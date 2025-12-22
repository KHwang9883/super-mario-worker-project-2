using Godot;
using SMWP.Level.Data;
using SMWP.Level.Physics;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;
    
public partial class SetJumpHeightMetaProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        if (float.TryParse(metadata, out var jumpHeight)) {
            if (node.TryGetComponent(out TroopaFlyBlueMovement? movement)) {
                movement.Height = jumpHeight;
            }
        }
    }
}