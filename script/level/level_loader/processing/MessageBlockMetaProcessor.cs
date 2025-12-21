using Godot;
using SMWP.Level.Data;
using SMWP.Util;

namespace SMWP.Level.Loader.Processing;

public partial class MessageBlockMetaProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        if (node.TryGetComponent(out MessageBlock? block)) {
            block.InitMessage(instance.Metadata);
        }
    }
}