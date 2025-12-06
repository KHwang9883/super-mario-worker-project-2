using Godot;
using System;

public partial class PassageOut : Node2D {
    [Export] public int PassageId;
    
    [Export] public PassageIn.PassageDirection Direction;
}
