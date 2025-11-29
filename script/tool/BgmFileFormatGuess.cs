using System.IO;
using System.Linq;
using Godot;
using FileAccess = Godot.FileAccess;

namespace SMWP.Level.Tool;

public static class BgmFileFormatGuess {
    public enum BgmFileTypeEnum {
        Mp3,
        Wav,
        Ogg,
        Invalid,
    }
    private static readonly string[] PossibleFormats = [
        ".dll",
        ".it",
        ".xm",
        ".mod",
        ".mp3",
        ".wav",
        ".ogg",
    ];

    public static string GetFullBgmFileName(string path) {
        // 第一步：猜测可能的文件后缀，直到文件不为空
        foreach (var possibleFormat in PossibleFormats) {
            var tryPath = path + possibleFormat;
            var file = FileAccess.Open(tryPath, FileAccess.ModeFlags.Read);
            if (file == null) continue;
            return tryPath;
        }
        return null;
    }
    public static BgmFileTypeEnum GetGuessFormat(string path) {
        // 第二步：读取到文件，根据文件头猜测正确的格式
        var file = FileAccess.Open(GetFullBgmFileName(path), FileAccess.ModeFlags.Read);
        if (file != null) {
            var header = file.GetBuffer(12); // 读取前12字节

            // 检查常见音频格式的文件头
            if (header.Length < 4) return BgmFileTypeEnum.Invalid;

            // WAV 格式: RIFF header
            if (header.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 })) // "RIFF"
            {
                if (header.Length >= 12 &&
                    header.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x41, 0x56, 0x45 })) // "WAVE"
                    return BgmFileTypeEnum.Wav;
            }

            // OGG 格式: OggS header
            if (header.Take(4).SequenceEqual(new byte[] { 0x4F, 0x67, 0x67, 0x53 })) // "OggS"
                return BgmFileTypeEnum.Ogg;

            // MP3 格式: ID3 header 或 MPEG frame header
            if (header.Take(3).SequenceEqual(new byte[] { 0x49, 0x44, 0x33 }) || // "ID3"
                (header[0] == 0xFF && (header[1] & 0xE0) == 0xE0)) // MPEG frame
                return BgmFileTypeEnum.Mp3;

            // 文件不合法，返回 Invalid，使用内置 BGM
            // IT XM MOD 格式由于扩展类只能供 GDScript 使用，返回 Invalid，使用内置 BGM
        }
        return BgmFileTypeEnum.Invalid;
    }
}
