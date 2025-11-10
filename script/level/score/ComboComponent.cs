using Godot;
using System;
using System.Collections.Generic;

public partial class ComboComponent : Node {
    [Signal]
    public delegate void PlaySoundKickEventHandler();
    [Signal]
    public delegate void PlaySound1UPEventHandler();
    
    public int Combo { get; set; }
    public int Score { get; set; }

    public readonly Dictionary<int, int> ComboToScore = new Dictionary<int, int>
    {
        {1, 100},
        {2, 200},
        {3, 500},
        {4, 1000},
        {5, 2000},
        {6, 5000},
        {7, -1},
    };
    public int AddCombo() {
        Combo++;
        if (Combo > 7) {
            EmitSignal(SignalName.PlaySound1UP);
            Combo = 1;
        } else {
            EmitSignal(SignalName.PlaySoundKick);
        }
        Score = ComboToScore[Combo];
        return Score;
    }
}
