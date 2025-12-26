using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SMWP;

[GlobalClass]
public partial class SmwsLoader : Node {
    public GDC.Array<string> ErrorMessage { get; } = [];
    public volatile int LineNum;

    public async ValueTask<bool> Load(Stream compressedSmws) {
        ErrorMessage.Clear();
        LineNum = 0;
        
        // 用缓冲区包装
        var buffered = new BufferedStream(compressedSmws);
        // 检测文件类型是否为压缩的 smws，并构建解压后的纯文本流
        var decompressed = (Stream)(IsBinaryFile(compressedSmws)
            ? new GZipStream(buffered, CompressionMode.Decompress)
            : buffered);
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = new StreamReader(decompressed, Encoding.GetEncoding("gb18030"));
        
        return await Load0(reader);
    }

    private async ValueTask<bool> Load0(TextReader reader) {
        // 清空之前的行号记录
        GameManager.ScenarioNewLevelLineNum.Clear();
        
        // 第一行：Lives读取
        var livesLine = await ReadLineAsync(reader);
        if (livesLine == null) {
            ErrorMessage.Add("Failed to read lives at line 1");
            return false;
        }
        
        if (int.TryParse(livesLine, out int lives)) {
            GameManager.Life = lives;
        } else {
            ErrorMessage.Add($"Invalid lives format: {livesLine} at line {LineNum}");
            return false;
        }

        // 第二行：应该是"New Level"
        var newLevelLine = await ReadLineAsync(reader);
        if (newLevelLine == null) {
            ErrorMessage.Add("Failed to read second line");
            return false;
        }

        // 检查第二行是否为"New Level"
        if (!newLevelLine.StartsWith("New Level")) {
            // 如果不是"New Level"，则可能是自定义BGM包名
            GameManager.CustomBgmPackage = newLevelLine;
            
            // 读取第三行，应该是"New Level"
            newLevelLine = await ReadLineAsync(reader);
            if (newLevelLine == null || !newLevelLine.StartsWith("New Level")) {
                ErrorMessage.Add($"Expected 'New Level' at line {LineNum}, but got {newLevelLine}");
                return false;
            }
        } else {
            // 第二行就是"New Level"，没有自定义BGM包名
            GameManager.CustomBgmPackage = "";
        }

        int levelCount = 0;
        
        // 记录第一个New Level的行号
        LinesSet(levelCount, LineNum);
        levelCount++;

        // 继续读取后续的New Level行
        string? line;
        while ((line = await ReadLineAsync(reader)) != null) {
            if (line.StartsWith("New Level")) {
                LinesSet(levelCount, LineNum);
                levelCount++;
            }
        }

        return true;
    }

    private static bool IsBinaryFile(Stream smws) {
        if (!smws.CanSeek) {
            throw new NotSupportedException("Unseekable file stream: unable to detect smws type");
        }
        
        using var reader = new StreamReader(new GZipStream(smws, CompressionMode.Decompress, true));
        bool result;
        try {
            reader.Read();
            result = true;
        } catch (InvalidDataException) {
            result = false;
        }
        smws.Seek(0, SeekOrigin.Begin);
        return result;
    }

    public void LinesSet(int levelCount, int lineCount) {
        GameManager.ScenarioNewLevelLineNum[levelCount] = lineCount;
    }

    private async Task<string?> ReadLineAsync(TextReader reader) {
        Interlocked.Increment(ref LineNum);
        return await reader.ReadLineAsync();
    }
}