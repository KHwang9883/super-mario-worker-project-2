using System.Collections.Generic;
using Godot;
using SMWP.Level.Data;

namespace SMWP.Level.Loader.Processing;

public partial class TroopaShellProcessor : ObjectProcessor {
    [Export] public PackedScene[] PrefabByType { get; set; } = [];
    
    public override IEnumerable<Node>? CreateInstance(SmwpObjectDatabaseEntry definition, ClassicSmwlObject instance) {
        if (MetadataLengthIsInvalid("turtle shell", instance.Metadata, 1)) {
            return base.CreateInstance(definition, instance);
        }
        int type = ClassicSmwpCodec.Base62Decode(instance.Metadata[0]);
        if (type < 0 || type >= PrefabByType.Length) {
            GD.PushError($"Unknown shell type {type}");
            return base.CreateInstance(definition, instance);
        }
        return [PrefabByType[type].Instantiate()];
    }
}