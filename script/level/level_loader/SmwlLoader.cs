using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using SMWP.Level.Data;
using SMWP.Util;

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
        // 用缓冲区包装
        var buffered = new BufferedStream(compressedSmwl);
        // 检测文件类型是否为压缩的 smwl，并构建解压后的纯文本流
        var decompressed = (Stream) (IsBinaryFile(compressedSmwl)
            ? new GZipStream(buffered, CompressionMode.Decompress)
            : buffered);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Load0(new StreamReader(decompressed, Encoding.GetEncoding("gb18030")));
    }

    private static bool IsBinaryFile(Stream smwl) {
        if (!smwl.CanSeek) {
            throw new NotSupportedException("Unseekable file stream: unable to detect smwl type");
        }
        using var reader = new StreamReader(new GZipStream(smwl, CompressionMode.Decompress, true));
        bool result;
        try {
            reader.Read();
            result = true;
        } catch (InvalidDataException) {
            result = false;
        }
        smwl.Seek(0, SeekOrigin.Begin);
        return result;
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
        // 解析 Additional Settings 数据
        var addition = await ParseAddition(reader);
        if (addition == null) {
            return null;
        }
        // 解析 SMWP 2.0 的新配置项
        var v2Meta = await ParseV2Metadata(reader);
        return new SmwlLevelData {
            Header = header,
            AdditionalSettings = addition,
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
        int time = 0;
        float gravity = 0, waterLevel = 0;
        int bossEnergy = 0;
        success = success && int.TryParse(line = await ReadLineAsync(reader), out time);
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
            RoomWidth = w,
            RoomHeight = h,
            LevelTitle = title!,
            LevelAuthor = author!,
            Time = time,
            Gravity = gravity,
            KoopaEnergy = bossEnergy,
            WaterHeight = waterLevel,
            // 将默认背景设置为 1 号背景，BGM 同理
            BgpId = bgp > 0 ? bgp : 1,
            BgmId = bgm > 0 ? bgm : 1,
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
            var first = (char)reader.Peek();
            // 有的 smwl/mfl 的对象区里有空行，
            // 所以遇到空行的时候不能停止对象加载。
            if (!char.IsNumber(first) && !char.IsWhiteSpace(first)) {
                break;
            }
            var line = await ReadLineAsync(reader);
            // 关卡文件结尾，跳出循环
            if (line == null) {
                break;
            }
            // 将水管连接的 id 转换为 499 以方便处理
            if (first == '4') {
                line = line.Insert(1, "99");
            }
            // 长度小于 11 的行为错误行，记录错误
            if (line.Length < 11) {
                ErrorMessage.Add($"Insufficient length for object {line} at line {LineNum}");
                continue;
            }
            var success = true;
            int id = -1;
            var position = Vector2I.Zero;
            success = success && int.TryParse(line.AsSpan()[..3], out id);
            success = success && ClassicSmwpCodec.TryDecodeCoordinate(line.AsSpan()[3..11], out position);
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

    private async ValueTask<ClassicSmwlAdditionalSettingsData?> ParseAddition(TextReader reader) {
        var config = new System.Collections.Generic.Dictionary<string, string>();
    
        // 读取所有配置行
        while (true) {
            var line = await ReadLineAsync(reader);
            if (string.IsNullOrEmpty(line) || !line.Contains('='))
                break;  // 遇到非配置行或空行结束
        
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0) {
                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();
                config[key] = value;
            }
        }

        return new ClassicSmwlAdditionalSettingsData {
            ModifiedMovement = config.GetValueOrDefault("modifiedmov") == "1",
            RotoDiscLayer = config.GetValueOrDefault("rotodisclay") == "1",
            LayerOrder = config.GetValueOrDefault("layerord") switch {
                "0" => LevelConfig.LayerOrderEnum.Classic,
                "1" => LevelConfig.LayerOrderEnum.WaterAbove,
                "2" => LevelConfig.LayerOrderEnum.Modified,
                _ => LevelConfig.LayerOrderEnum.Classic,
            },
            FluidType = config.GetValueOrDefault("lava") switch {
                "0" => Fluid.FluidTypeEnum.Water,
                "1" => Fluid.FluidTypeEnum.Lava,
                _ => Fluid.FluidTypeEnum.Water,
            },
            AutoFluid = config.GetValueOrDefault("auto") == "1",
            FluidT1 = ConfigurationExtensions.GetIntValueOrDefault(config, "T1", 0),
            FluidT2 = ConfigurationExtensions.GetIntValueOrDefault(config, "T2", -64),
            FluidSpeed = ConfigurationExtensions.GetIntValueOrDefault(config, "velocity", 1),
            FluidDelay = ConfigurationExtensions.GetIntValueOrDefault(config, "delay", 0),
            AdvancedSwitch = config.GetValueOrDefault("advswitch") == "1",
            FastRetry = config.GetValueOrDefault("fastretry") == "1",
            MfStyleBeet = config.GetValueOrDefault("MFbeet") == "1",
            CelesteStyleSwitch = config.GetValueOrDefault("celeste") == "1",
            MfStylePipeExit = config.GetValueOrDefault("pipeout") == "1",
            FasterLevelPass = config.GetValueOrDefault("fastpass") == "1",
            HUDDisplay = config.GetValueOrDefault("huddisplay", "0") == "0",
            RainyLevel = ConfigurationExtensions.GetIntValueOrDefault(config, "rainy", 0),
            FallingStarsLevel = ConfigurationExtensions.GetIntValueOrDefault(config, "fallingstars", 0),
            SnowyLevel = ConfigurationExtensions.GetIntValueOrDefault(config, "snowy", 0),
            ThunderLevel = ConfigurationExtensions.GetIntValueOrDefault(config, "thunder", 0),
            WindyLevel = ConfigurationExtensions.GetIntValueOrDefault(config, "windy", 0),
            Darkness = ConfigurationExtensions.GetIntValueOrDefault(config, "darkness", 0),
            Brightness = ConfigurationExtensions.GetIntValueOrDefault(config, "brightness", 0),
            
            // Todo: lightobject
            // lightobject
            //lightobject=0000000000000000000000000000000000000000000000000000000000000000000
            //
            
            ThwompActivateBlocks = config.GetValueOrDefault("stunblock") == "1",
            SmwpVersion = ConfigurationExtensions.GetIntValueOrDefault(config, "version", 0)
                          // 部分早期版本版本号为五位数，实际上读取的时候只读取前四位
                          % 10000,
        };
    }

    public async ValueTask<GDC.Dictionary<string, string>> ParseV2Metadata(TextReader reader) {
        GDC.Dictionary<string, string> result = [];
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