using Godot;
using System;
using SMWP.Level.Data;
using SMWP.Level.Loader.Processing;

public partial class SceneControlMetaProcessor : ObjectProcessor {
    public override void ProcessObject(Node node, ClassicSmwlObject instance) {
        if (node is not SceneControl sceneControl) return;
        var metadata = instance.Metadata;
        
        int.TryParse(metadata[..1], out var changeBgm);
        sceneControl.ChangeBgm = changeBgm == 1;
        int.TryParse(metadata[1..4], out var bgmId);
        sceneControl.BgmId = bgmId;
        int.TryParse(metadata[4..5], out var changeBgp);
        sceneControl.ChangeBgp = changeBgp == 1;
        int.TryParse(metadata[5..7], out var bgpId);
        sceneControl.BgpId = bgpId;
        int.TryParse(metadata[7..8], out var linkedWith);
        sceneControl.LinkedWithObject = linkedWith switch {
            0 => SceneControl.LinkedWithObjectEnum.None,
            1 => SceneControl.LinkedWithObjectEnum.ViewControl,
            2 => SceneControl.LinkedWithObjectEnum.Koopa,
            _ => throw new ArgumentOutOfRangeException(),
        };
        if (!ClassicSmwpCodec.TryDecodeValue(metadata.AsSpan()[8..12], 4, out int waterHeight)) {
            GD.PushError($"Invalid water height {metadata[8..12]} for scene control");
        }
        sceneControl.WaterHeight = waterHeight;
        
        // SMWP v1.7.12 之前的版本没有天气参数
        if (metadata.Length < 13) return;
        int.TryParse(metadata[12..13], out var changeWeather);
        sceneControl.ChangeWeather = changeWeather == 1;
        int.TryParse(metadata[13..14], out var rainyLevel);
        sceneControl.RainyLevel = rainyLevel;
        int.TryParse(metadata[14..15], out var fallingStarsLevel);
        sceneControl.FallingStarsLevel = fallingStarsLevel;
        int.TryParse(metadata[15..16], out var snowyLevel);
        sceneControl.SnowyLevel = snowyLevel;
        int.TryParse(metadata[16..17], out var thunderLevel);
        sceneControl.ThunderLevel = thunderLevel;
        int.TryParse(metadata[17..18], out var windyLevel);
        sceneControl.WindyLevel = windyLevel;
        int.TryParse(metadata[18..19], out var darkness);
        sceneControl.Darkness = darkness;
        int.TryParse(metadata[19..20], out var brightness);
        sceneControl.Brightness = brightness;
    }
}
