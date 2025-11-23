using Godot;
using System;

public partial class ThunderController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 1, 1")] public int ThunderLevel;

}
