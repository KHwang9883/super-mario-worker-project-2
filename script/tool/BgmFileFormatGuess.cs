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
        It,
        Xm,
        Mod,
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
        return null!;
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
            // IT 格式: IMPM header
            if (header.Length >= 4 && header.Take(4).SequenceEqual(new byte[] { 0x49, 0x4D, 0x50, 0x4D })) // "IMPM"
                return BgmFileTypeEnum.It;
                
            // XM 格式: Extended Module header
            if (header.Length >= 17 && header.Take(17).SequenceEqual(new byte[] { 0x45, 0x78, 0x74, 0x65, 0x6E, 0x64, 0x65, 0x64, 0x20, 0x4D, 0x6F, 0x64, 0x75, 0x6C, 0x65, 0x3A, 0x20 })) // "Extended Module: "
                return BgmFileTypeEnum.Xm;
                
            // MOD 格式检测
            // 对于 MOD 文件，我们需要检查文件长度和魔数
            // 首先检查文件是否足够大以容纳 MOD 头信息
            if (file.GetLength() >= 1084) {
                // 保存当前位置
                var currentPosition = file.GetPosition();
                // 移动到魔数位置（偏移量1080）
                file.Seek(1080);
                // 读取4字节魔数
                var modMagic = file.GetBuffer(4);
                // 恢复位置
                file.Seek(currentPosition);
                
                // 检查标准 MOD 魔数
                if (modMagic.Length < 4) return BgmFileTypeEnum.Invalid;
                // ProTracker/FastTracker 魔数模式
                bool isModMagic =
                    (modMagic[0] == 0x4D && modMagic[1] == 0x2E && modMagic[2] == 0x4B && modMagic[3] == 0x2E) || // M.K.
                    (modMagic[0] == 0x4D && modMagic[1] == 0x21 && modMagic[2] == 0x4B && modMagic[3] == 0x21) || // M!K!
                    (modMagic[0] == 0x34 && modMagic[1] == 0x43 && modMagic[2] == 0x48 && modMagic[3] == 0x4E) || // 4CHN
                    (modMagic[0] == 0x36 && modMagic[1] == 0x43 && modMagic[2] == 0x48 && modMagic[3] == 0x4E) || // 6CHN
                    (modMagic[0] == 0x38 && modMagic[1] == 0x43 && modMagic[2] == 0x48 && modMagic[3] == 0x4E) || // 8CHN
                    // 其他可能的魔数
                    (modMagic[0] == 0x46 && modMagic[1] == 0x43 && modMagic[2] == 0x48 && modMagic[3] == 0x4E) || // FCHN
                    (modMagic[0] == 0x54 && modMagic[1] == 0x43 && modMagic[2] == 0x48 && modMagic[3] == 0x4E);
                    
                // 检查常见的 MOD 魔数
                // M.K., M!K!, 4CHN, 6CHN, 8CHN 等
                    
                if (isModMagic) {
                    return BgmFileTypeEnum.Mod;
                }
            }
            
            // 文件不匹配任何已知格式，返回 Invalid
        }
        return BgmFileTypeEnum.Invalid;
    }
}
