using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

public partial class ShellMovingInteraction : Node {
    [Signal]
    public delegate void HardedEventHandler();
    [Export] private CharacterBody2D _shell = null!;
    [Export] private ComboComponent? _shellCombo;
    [Export] public int HardLevel = 5;
    private Node2D? _parent;

    public override void _Ready() {
        _parent ??= (Node2D)GetParent();
        _parent.SetMeta("ShellInteraction", this);
    }
    public override void _PhysicsProcess(double delta) {
        var results = ShapeQueryResult.ShapeQuery(_shell, _shell.GetNode<ShapeCast2D>("AreaBodyCollision"));

        foreach (var result in results) {
            // ShapeCast2D ExcludeParent 属性疑似无用，因此额外检查是不是自身
            if (result == _shell) continue;
            
            // 不与同级硬度的运动龟壳相撞
            if (result.HasMeta("ShellInteraction")) {
                var shellInteraction = (ShellMovingInteraction)result.GetMeta("ShellInteraction");
                if (HardLevel == shellInteraction.HardLevel) {
                    continue;
                }
                // 硬度较小（硬度较大则无事发生）
                if (HardLevel < shellInteraction.HardLevel) {
                    EmitSignal(SignalName.Harded);
                }
            }
            
            // 检测可以被龟壳击中的物件
            Node? interactionWithShellNode = null;
            
            if (result.HasMeta("InteractionWithShell")) {
                interactionWithShellNode = (Node)result.GetMeta("InteractionWithShell");
            }
            if (interactionWithShellNode is not IShellHittable shellHittable) continue;
            result.SetMeta("InteractingObject", _shell);
            if (!shellHittable.IsShellHittable) continue; 
            
            // 硬度检测
            if (HardLevel > shellHittable.HardLevel) {
                if (_shellCombo != null) {
                    shellHittable.OnShellHit(_shellCombo.AddCombo());
                }
            } else if (HardLevel == shellHittable.HardLevel) {
                if (_shellCombo != null) {
                    shellHittable.OnShellHit(_shellCombo.AddCombo());
                }
                // 硬度相同，同归于尽
                EmitSignal(SignalName.Harded);
            }
            if (HardLevel < shellHittable.HardLevel || shellHittable.KillShell) {
                // 硬度较小或者目标可以反死龟壳
                EmitSignal(SignalName.Harded);
            }
        }
    }
}
