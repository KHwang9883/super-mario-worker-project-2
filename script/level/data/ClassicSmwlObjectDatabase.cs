using Godot;
using System;
using SMWP.Level.Data;

[GlobalClass]
public partial class ClassicSmwlObjectDatabase : Resource {
    [Export] public ClassicSmwlObject[] Entries { get; private set; } = [];
}
