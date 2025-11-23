using Godot;
using System;

public partial class FallingStarsController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 3, 1")] public int FallingStarsLevel;

}
