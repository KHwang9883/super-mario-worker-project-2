using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class BgmDatabaseEntry : Resource {
    [Export] public int BgmId;

    public enum AlbumEnum {
        MW,
        MFR,
        SMS,
        SMS2,
        Softendo,
        OM,
        Boss,
        //Custom,
    }
    [Export] public AlbumEnum AlbumPath;

    public static Dictionary<AlbumEnum, string> AlbumToPath { get; } = new();

    static BgmDatabaseEntry() {
        foreach (var album in Enum.GetValues<AlbumEnum>()) {
            AlbumToPath[album] = album.ToString();
        }
    }
    
    [Export] public AudioStream DefaultBgm = null!;
    [Export] public string[] FileNameForOverride = [];
}
