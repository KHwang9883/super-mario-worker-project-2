using Godot;
using System;
using SMWP.Level.Interface;
using SMWP.Level.Physics;

public partial class ShellInteraction : Node {
    [Signal]
    public delegate void HardedEventHandler();
    [Export] private CharacterBody2D _shell = null!;
    
    [Export] private ComboComponent? _shellCombo;
    
    public override void _PhysicsProcess(double delta) {
        var results = ShapeQueryResult.ShapeQuery(_shell, _shell.GetNode<ShapeCast2D>("AreaBodyCollision"));

        foreach (var result in results) {
            // 检测可以被龟壳击中的物件
            
            // ShapeCast2D ExcludeParent 属性疑似无用，因此额外检查是不是自身
            if (result == _shell) continue;
            
            Node? interactionWithShellNode = null;
            
            if (result.HasMeta("InteractionWithShell")) {
                interactionWithShellNode = (Node)result.GetMeta("InteractionWithShell");
            }
            if (interactionWithShellNode is not IShellHittable shellHittable) continue;
            // 被硬物件反死
            if (_shellCombo == null || !shellHittable.OnShellHit(_shellCombo.AddCombo())) continue;
            EmitSignal(SignalName.Harded);
        }
    }
}
