using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;
using SMWP.Util;

public partial class PiranhaPlantProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        node.TryGetComponent(out PiranhaPlantMovement? piranhaPlantMovement);
        piranhaPlantMovement!.SetAngle(180f);
        if (node is not Node2D node2D) return;
        node2D.Position += Vector2.Down * 32f;
    }
}
