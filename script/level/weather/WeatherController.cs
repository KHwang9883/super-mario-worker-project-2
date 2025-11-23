using Godot;
using System;

public partial class WeatherController : Node {
    [Export] public RainyController rainyController = null!;
    [Export] public FallingStarsController fallingStarsController = null!;
    [Export] public SnowyController snowyController = null!;
    [Export] public ThunderController thunderController = null!;
    [Export] public WindyController windyController = null!;
    [Export] public DarkController darkController = null!;
    [Export] public BrightController brightController = null!;
}
