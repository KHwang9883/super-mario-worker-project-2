using Godot;
using System;
using SMWP.Level;

public partial class Checkpoint : Area2D {
    [Signal]
    public delegate void CheckpointActivatedEventHandler();
    
    [Export] public int Id;
    [Export] public bool Activated;

    public override void _Ready() {
        // Activate activated
        if (!LevelManager.ActivatedCheckpoints.Contains(Id)) return;
        Activated = true;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "activated";
    }
    public void OnBodyEntered(Node body) {
        if(!Activate()) return;
    }
    public bool Activate() {
        if (Activated) return false;
        Activated = true;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "activated";
        LevelManager.CurrentCheckpointId = Id;
        LevelManager.ActivatedCheckpoints.Add(Id);
        EmitSignal(SignalName.CheckpointActivated);
        return true;
    }
}
