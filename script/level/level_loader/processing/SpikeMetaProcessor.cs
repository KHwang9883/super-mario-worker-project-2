using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;

public partial class SpikeMetaProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        if (MetadataLengthIsInvalid("spike", instance.Metadata, 1)) {
            return;
        }
        if (node is not Node2D node2D) return;
        GD.Print(instance.Metadata[0]);
        if (!int.TryParse(instance.Metadata, out var direction)) return;
        node2D.GlobalRotationDegrees = direction switch {
            0 => 0f,
            1 => 180f,
            2 => 270f,
            3 => 90f,
            _ => 0f,
        };
        GD.Print(node2D.GlobalRotationDegrees);
    }
}
