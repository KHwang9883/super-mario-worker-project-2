using Godot;
using System;

public partial class WindyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 3, 1")] public int WindyLevel;

}
