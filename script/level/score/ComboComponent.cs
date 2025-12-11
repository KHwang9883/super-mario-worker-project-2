using Godot;
using System;
using System.Collections.Generic;

public partial class ComboComponent : Node {
    [Signal]
    public delegate void PlaySoundKickEventHandler();
    
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
        if (Combo > 7) Combo = 1;
        EmitSignal(SignalName.PlaySoundKick);
        // 1UP 音效的播放见 LevelManager 的 AddLife 方法
        Score = ComboToScore[Combo];
        return Score;
    }
    public void ResetCombo() {
        Combo = 0;
    }
}
