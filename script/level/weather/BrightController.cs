using Godot;
using System;

public partial class BrightController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 5, 1")] public int BrightLevel;
    
}
