using Godot;
using System;

public partial class SnowyController : Node {
    [Export] private WeatherController _weatherController = null!;
    [Export(PropertyHint.Range,"0, 5, 1")] public int SnowyLevel;

}
