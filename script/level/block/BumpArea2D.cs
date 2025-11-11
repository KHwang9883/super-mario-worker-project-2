using Godot;
using System;
using System.Threading;
using SMWP.Level.Interface;

namespace SMWP.Level.Block;

public partial class BumpArea2D : Node {
    [Export] private Area2D? _parent;
    private int _timer;
    
    public override void _Ready() {
        _parent ??= GetParent<Area2D>();
    }
    public override void _PhysicsProcess(double delta) {
        _timer++;
        if (_timer < 6) return;
        _parent?.QueueFree();
    }
    
    // 顶死敌人
    public void OnBodyEntered(Node2D node) {
        Node? interactionWithBumpNode = null;
        
        if (node.HasMeta("InteractionWithBump")) {
            interactionWithBumpNode = (Node)node.GetMeta("InteractionWithBump");
        }
        if (interactionWithBumpNode is IBumpHittable bumpHittable){
            bumpHittable.OnBumped();
        }
    }
}
