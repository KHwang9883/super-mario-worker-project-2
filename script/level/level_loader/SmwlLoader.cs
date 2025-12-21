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
        // 用缓冲区包装
        var buffered = new BufferedStream(compressedSmwl);
        // 检测文件类型是否为压缩的 smwl，并构建解压后的纯文本流
        var decompressed = (Stream) (IsBinaryFile(compressedSmwl)
            ? new GZipStream(buffered, CompressionMode.Decompress)
            : buffered);
        return Load0(new StreamReader(decompressed));
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

    private async ValueTask<ClassicSmwlAdditionalSettingsData?> ParseAddition(TextReader reader) {
        return new ClassicSmwlAdditionalSettingsData();
        var success = true;
        var line = "";

        // Todo: example
        /*
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
        int id = -1;
        var position = Vector2.Zero;
        success = success && int.TryParse(line.AsSpan()[..3], out id);
        success = success && float.TryParse(line.AsSpan()[3..7], out position.X);
        success = success && float.TryParse(line.AsSpan()[7..11], out position.Y);
        var metadata = line.Length == 11 ? "" : line[11..];
        */

        // modifiedmov
        //modifiedmov=1
        bool modifiedMovement = true;
        success = success && bool.TryParse(line.AsSpan()[13..14], out modifiedMovement);

        // rotodisclay
        //rotodisclay=0
        bool rotoDiscLayer = false;
        success = success && bool.TryParse(line.AsSpan()[13..14], out rotoDiscLayer);

        // layerord
        //layerord=2
        LevelConfig.LayerOrderEnum layerOrder = LevelConfig.LayerOrderEnum.Modified;
        success = success && int.TryParse(line.AsSpan()[5..6], out int value) &&
                  (layerOrder = value switch
                  {
                      0 => LevelConfig.LayerOrderEnum.Classic,
                      1 => LevelConfig.LayerOrderEnum.WaterAbove,
                      2 => LevelConfig.LayerOrderEnum.Modified,
                      _ => LevelConfig.LayerOrderEnum.Classic,
                  }) == layerOrder;

        // lava
        //lava=0
        Fluid.FluidTypeEnum fluidType = Fluid.FluidTypeEnum.Water;
        if (success && int.TryParse(line.AsSpan()[5..6], out var lavaValue)) {
            fluidType = lavaValue == 0 ? Fluid.FluidTypeEnum.Water : Fluid.FluidTypeEnum.Lava;
            success = true;
        } else {
            success = false;
        }

        // auto
        //auto=0
        bool autoFluid = false;

        // T1
        //T1=0
        float t1 = 0f;

        // T2
        //T2=-64
        float t2 = -64f;

        // velocity
        //velocity=1
        float fluidSpeed = 1f;

        // delay
        //delay=0
        int fluidDelay = 0;

        // advswitch
        //advswitch=0
        bool advancedSwitch = false;

        // fastretry
        //fastretry=0
        bool fastRetry = false;

        // MFbeet
        //MFbeet=1
        bool mfStyleBeet = true;

        // celeste
        //celeste=1
        bool celesteStyleSwitch = true;

        // pipeout
        //pipeout=0
        bool mfStylePipeExit = false;

        // fastpass
        //fastpass=0
        bool fasterLevelPass = false;

        // huddisplay
        //huddisplay=0
        bool hudDisplay = false;

        // rainy
        //rainy=0
        int rainyLevel = 0;

        // fallingstars
        //fallingstars=0
        int fallingStarsLevel = 0;

        // snowy
        //snowy=0
        int snowyLevel = 0;

        // thunder
        //thunder=0
        int thunderLevel = 0;

        // windy
        //windy=0
        int windyLevel = 0;

        // darkness
        //darkness=0
        int darkness = 0;

        // brightness
        //brightness=0
        int brightness = 0;

        // lightobject
        //lightobject=0000000000000000000000000000000000000000000000000000000000000000000
        string lightObject = "";

        // stunblock
        //stunblock=0
        bool thwompActivateBlocks = false;

        // version
        //version=1712
        int smwpVersion;

        if (!success) {
            ErrorMessage.Add($"Corrupted SMWL additional settings {line} at line {LineNum}");
            return null;
        }
        return new ClassicSmwlAdditionalSettingsData {
            ModifiedMovement = modifiedMovement,
            RotoDiscLayer = rotoDiscLayer,
            LayerOrder = layerOrder,
            FluidType = fluidType,
            AutoFluid = autoFluid,
            FluidT1 = t1,
            FluidT2 = t2,
            FluidSpeed = fluidSpeed,
            FluidDelay = fluidDelay,
            AdvancedSwitch = advancedSwitch,
            FastRetry = fastRetry,
            MfStyleBeet = mfStyleBeet,
            CelesteStyleSwitch = celesteStyleSwitch,
            MfStylePipeExit = mfStylePipeExit,
            FasterLevelPass = fasterLevelPass,
            HUDDisplay = hudDisplay,
            RainyLevel = rainyLevel,
            FallingStarsLevel = fallingStarsLevel,
            SnowyLevel = snowyLevel,
            ThunderLevel = thunderLevel,
            WindyLevel = windyLevel,
            Darkness = darkness,
            Brightness = brightness,

            // Todo: lightobject
            //

            ThwompActivateBlocks = thwompActivateBlocks,
        };

        // Placeholder
        return new ClassicSmwlAdditionalSettingsData();
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