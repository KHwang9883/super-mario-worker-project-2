using Godot;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using SMWP.Level.Data;

namespace SMWP.Level;

[GlobalClass]
public partial class SmwsLoader : Node {
    public GDC.Array<string> ErrorMessage { get; } = [];

    public async ValueTask<SmwsScenarioData?> Load(Stream compressedSmws, SmwlLoader levelLoader) {
        ErrorMessage.Clear();
        
        // 用缓冲区包装
        var buffered = new BufferedStream(compressedSmws);
        // 检测文件类型是否为压缩的 smws，并构建解压后的纯文本流
        var decompressed = (Stream)(LoaderHelper.IsBinaryFile(compressedSmws)
            ? new GZipStream(buffered, CompressionMode.Decompress)
            : buffered);
        
        return await Load0(new StreamReader(decompressed, LoaderHelper.GetGb18030()), levelLoader);
    }

    private async ValueTask<SmwsScenarioData?> Load0(TextReader reader, SmwlLoader levelLoader) {
        string? current;
        if (int.TryParse(current = await reader.ReadLineAsync(), out int lives)) {
            ErrorMessage.Add($"Invalid live number line, found {current}");
            return null;
        }
        GDC.Array<SmwlLevelData> levels = [];

        levelLoader.ErrorMessage.Clear();
        while (reader.Peek() >= 0) {
            var line = await reader.ReadLineAsync();
            if (line == null) {
                break;
            }
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            if (line == "New Level") {
                if (await levelLoader.Load(reader) is { } level) {
                    levels.Add(level);
                }
            }
            ErrorMessage.Add($"Unexpected line \"{line}\" in smws scenario");
        }
        
        ErrorMessage.AddRange(levelLoader.ErrorMessage);
        levelLoader.ErrorMessage.Clear();

        return new SmwsScenarioData {
            Header = new ClassicSmwsHeaderData {
                Lives = lives,
            },
            Levels = levels,
        };
    }
}