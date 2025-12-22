using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;

public partial class TroopaFlyYellowGeneratorProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        var metadata = instance.Metadata;
        // 检查元数据长度
        if (MetadataLengthIsInvalid("fly troopa gold generator", metadata, 10)) {
            return;
        }
        // 给金飞龟生成器属性赋值
        if (node is not TroopaFlyYellowGenerator troopaFlyYellowGenerator) return;
        
        // 金飞龟生成器半径
        if (float.TryParse(metadata[..3], out var radius)) {
            troopaFlyYellowGenerator.Radius = radius;
        }
        // 金飞龟生成器初始角度
        if (float.TryParse(metadata[3..6], out var angle)) {
            troopaFlyYellowGenerator.Angle = angle;
        }
        // 金飞龟生成器飞龟数量
        if (int.TryParse(metadata[6..9], out var amount)) {
            troopaFlyYellowGenerator.Amount = amount;
        }
        // 金飞龟生成器朝向
        if (int.TryParse(metadata[9..10], out var direction)) {
            troopaFlyYellowGenerator.Direction = (direction == 0);
        }
    }
}
