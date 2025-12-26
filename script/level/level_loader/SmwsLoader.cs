using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using SMWP.Level.Data;

[GlobalClass]
public partial class SmwsLoader : Node {
    public GDC.Array<string> ErrorMessage { get; } = [];

}
