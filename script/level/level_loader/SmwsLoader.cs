using Godot;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using SMWP.Level.Data;

namespace SMWP.Level;

[GlobalClass]
public partial class SmwsLoader : Node {
    public GDC.Array<string> ErrorMessage { get; } = [];

    public async ValueTask<SmwsScenarioData?> Load(Stream file, SmwlLoader levelLoader) {
        ErrorMessage.Clear();
        
        // 用缓冲区包装
        var buffered = LoaderHelper.MakeBuffered(await LoaderHelper.MakeSeekableAsync(file));
        // 检测文件类型是否为压缩的 smwl，并构建解压后的纯文本流
        var decompressed = LoaderHelper.IsBinaryFile(file)
            ? await LoaderHelper.MakeSeekableAsync(new GZipStream(buffered, CompressionMode.Decompress))
            : buffered;
        
        return await Load0(new StreamReader(decompressed, LoaderHelper.GetGb18030()), levelLoader);
    }

    private async ValueTask<SmwsScenarioData?> Load0(TextReader reader, SmwlLoader levelLoader) {
        string? line1;
        if (!int.TryParse(line1 = await reader.ReadLineAsync(), out int lives)) {
            ErrorMessage.Add($"Invalid live number line, found {line1}");
            return null;
        }
        GDC.Array<SmwlLevelData> levels = [];
        
        var line2 = await reader.ReadLineAsync();
        if (line2 != "New Level") {
            return null;
        }

        levelLoader.ErrorMessage.Clear();
        while (reader.Peek() >= 0) {
            if (await levelLoader.Load(reader) is { } level) {
                levels.Add(level);
                GameManager.ScenarioLevelCount++;
            }
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