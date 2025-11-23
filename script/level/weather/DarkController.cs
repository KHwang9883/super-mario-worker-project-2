using Godot;
using System;

public partial class DarkController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 9, 1")] public int DarkLevel;

}
