using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;
using SMWP.Util;

public partial class BulletBillCannonProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        node.TryGetComponent(out Sprite2D? sprite);
        sprite!.FlipV = true;
    }
}
