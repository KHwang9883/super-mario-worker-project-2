using Godot;
using System;
using SMWP.Level.Sound;

public partial class Logo : Node2D {
    [Export] private string _titleUid = "uid://2h2s1iqemydd";

    public override void _Ready() {
        GetTree().GetRoot().GetNode<AudioStreamPlayer>("/root/SoundManager/SoundINL").Playing = true;
    }
    public override void _PhysicsProcess(double delta) {
        if (Input.IsAnythingPressed()) JumpToScene();
    }
    public void OnAnimationFinished() {
        JumpToScene();
    }
    public void JumpToScene() {
        GetTree().ChangeSceneToFile(_titleUid);
    }
}
