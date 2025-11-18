using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using SMWP.Level.Data;

namespace SMWP.Level;

[GlobalClass]
public partial class SmwlLoader : Node {
    public const string BlockDataStart = "BlocksDataStart";
    public const string BlockDataEnd = "BlocksEnd";

    public Array<string> ErrorMessage { get; } = [];
    public volatile int LineNum;

    public ValueTask<SmwlLevelData?> Load(string content) {
        return Load0(new StringReader(content));
    }

    public ValueTask<SmwlLevelData?> Load(Stream compressedSmwl) {
        return Load0(new StreamReader(new GZipStream(new BufferedStream(compressedSmwl), CompressionMode.Decompress)));
    }
    
    private async ValueTask<SmwlLevelData?> Load0(TextReader reader) {
        // 解析文件头
        var header = await ParseHeader(reader);
        if (header == null) {
            return null;
        }
        // 解析 Block 数据
        var blocks = await ParseBlocks(reader);
        if (blocks == null) {
            return null;
        }
        // 解析 Objects 数据
        var objects = await ParseObjects(reader);
        // 解析 SMWP 二期的新配置项
        var v2Meta = await ParseV2Metadata(reader);
        return new SmwlLevelData {
            Header = header,
            Blocks = blocks,
            Objects = objects,
            V2Metadata = v2Meta,
        };
    }

    private async ValueTask<ClassicSmwlHeaderData?> ParseHeader(TextReader reader) {
        var success = true;
        var line = "";
        // 长，宽
        int w = 0, h = 0;
        success = success && int.TryParse(line = await ReadLineAsync(reader), out w);
        success = success && int.TryParse(line = await ReadLineAsync(reader), out h);
        // 关卡标题和作者
        string? title = null, author = null;
        success = success && (title = line = await ReadLineAsync(reader)) != null;
        success = success && (author = line = await ReadLineAsync(reader)) != null;
        // 关卡时间，重力，boss 血量，水位深度
        float time = 0, gravity = 0, waterLevel = 0;
        int bossEnergy = 0;
        success = success && float.TryParse(line = await ReadLineAsync(reader), out time);
        success = success && float.TryParse(line = await ReadLineAsync(reader), out gravity);
        success = success && int.TryParse(line = await ReadLineAsync(reader), out bossEnergy);
        success = success && float.TryParse(line = await ReadLineAsync(reader), out waterLevel);
        // BGP, BGM
        int bgp = -1, bgm = -1;
        success = success && int.TryParse(line = await ReadLineAsync(reader), out bgp);
        success = success && int.TryParse(line = await ReadLineAsync(reader), out bgm);

        if (!success) {
            ErrorMessage.Add($"Corrupted SMWL header {line} at line {LineNum}");
            return null;
        }
        return new ClassicSmwlHeaderData {
            Width = w,
            Height = h,
            Title = title!,
            Author = author!,
            LevelTime = time,
            Gravity = gravity,
            BossEnergy = bossEnergy,
            WaterLevel = waterLevel,
            LevelBackground = bgp,
            BackgroundMusic = bgm,
        };
    }

    private async ValueTask<ClassicSmwlBlocksData?> ParseBlocks(TextReader reader) {
        // 检查 BlocksDataStart
        if (!await Check(reader, BlockDataStart)) {
            return null;
        }
        Array<byte[]> buffer = [];
        int width = -1;
        while (true) {
            var line = await ReadLineAsync(reader);
            if (string.IsNullOrEmpty(line) || line == BlockDataEnd) {
                break;
            }
            // 当前行字符数量为奇数时说明关卡文件损坏，因为 2 个字符组成一个 block.
            if ((line.Length & 1) != 0) {
                ErrorMessage.Add($"Found lines with odd length in block data at line {LineNum}");
                return null;
            }
            // 计算 / 验证长度
            if (width < 0) {
                width = line.Length >> 1;
            } else if (width != line.Length >> 1) {
                ErrorMessage.Add($"Block width not the same: expect {width}, found {line.Length >> 1} at line {LineNum}");
                return null;
            }
            // 转换为 byte 并存入当前行
            var blockLine = new byte[line.Length];
            for (var i = 0; i < line.Length; i++) {
                blockLine[i] = (byte)line[i];
            }
            buffer.Add(blockLine);
        }
        return new ClassicSmwlBlocksData(buffer, width);
    }

    private async ValueTask<Array<ClassicSmwlObject>> ParseObjects(TextReader reader) {
        Array<ClassicSmwlObject> result = [];
        while (true) {
            // 检测到 SMWP 二期的扩展关卡数据则返回
            if (!char.IsNumber((char)reader.Peek())) {
                break;
            }
            var line = await ReadLineAsync(reader);
            // 关卡文件结尾，跳出循环
            if (line == null) {
                break;
            }
            // 长度小于 11 的行为错误行，记录错误
            if (line.Length < 11) {
                ErrorMessage.Add($"Insufficient length for object {line} at line {LineNum}");
                continue;
            }
            var success = true;
            int id = -1;
            var position = Vector2.Zero;
            success = success && int.TryParse(line.AsSpan()[..3], out id);
            success = success && float.TryParse(line.AsSpan()[3..7], out position.X);
            success = success && float.TryParse(line.AsSpan()[7..11], out position.Y);
            var metadata = line.Length == 11 ? "" : line[11..];
            if (success) {
                result.Add(new ClassicSmwlObject {
                    Id = id,
                    Position = position,
                    Metadata = metadata,
                });
            } else {
                ErrorMessage.Add($"Failed to parse object {line} at line {LineNum}");
            }
        }
        return result;
    }

    public async ValueTask<Dictionary<string, string>> ParseV2Metadata(TextReader reader) {
        Dictionary<string, string> result = [];
        while (true) {
            var line = await ReadLineAsync(reader);
            if (line == null) {
                break;
            }
            // 关卡文件结尾，跳出循环
            int separator;
            if ((separator = line.IndexOf('=')) < 0) {
                ErrorMessage.Add($"'=' not found in metadata {line} at line {LineNum}");
                continue;
            }
            result[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }
        return result;
    }

    private async ValueTask<bool> Check(TextReader reader, string expected) {
        var line = await ReadLineAsync(reader);
        if (line == expected) {
            return true;
        }
        ErrorMessage.Add($"Magic string check failed: expect {expected}, found {line} at line {LineNum}");
        return false;
    }

    /// <summary>
    /// 从 <param name="reader">Reader</param> 中读取一行并使行号 +1
    /// </summary>
    private Task<string?> ReadLineAsync(TextReader reader) {
        Interlocked.Increment(ref LineNum);
        return reader.ReadLineAsync();
    }
}