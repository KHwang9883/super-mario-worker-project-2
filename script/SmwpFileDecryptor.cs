using Godot;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SMWP;

public partial class SmwpFileDecryptor : Node {
    // GZip魔数（用于验证解压前的数据是否为合法GZip格式）
    private static readonly byte[] GZipMagicNumber = [0x1F, 0x8B];

    public override void _Ready() {
        base._Ready();
        
        // 提前注册编码提供程序（双重保障）
        try {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        } catch {
            // ignored
        }

        GetWindow().FilesDropped += files => {
            // 异步方法封装，避免void异步的异常丢失
            _ = OnSmwpFileDropped(files[0]);
        };
    }
    
    /// <summary>
    /// 文件拖拽放下后的处理逻辑：读取文件 → XOR解密 → 验证GZip格式 → GZip解压 → 转文本
    /// </summary>
    /// <param name="file">拖拽的文件路径</param>
    public async Task OnSmwpFileDropped(string file) {
        try {
            if (!File.Exists(file)) {
                GD.PrintErr($"文件不存在：{file}");
                return;
            }

            // 读取原始加密文件字节
            byte[] originalBytes = await File.ReadAllBytesAsync(file);
            //GD.Print($"读取文件成功，字节长度：{originalBytes.Length}");
            
            // 获取密钥
            DecryptKey.GetKey();
            
            var gameId = DecryptKey.GameId;
            var cryptGmidInit = DecryptKey.CryptGmidInit;
            var keyStr = DecryptKey.KeyStr;
            
            // 先执行 XOR 解密
            byte[] decryptedBytes = Xor.ScriptTextCryptBytes(
                originalBytes, keyStr, gameId, cryptGmidInit
                );
            //GD.Print($"XOR解密完成，解密后字节长度：{decryptedBytes.Length}");

            // 验证解密后的数据是否为合法 GZip 格式
            if (!IsValidGZipFormat(decryptedBytes)) {
                GD.PushWarning("解密后的数据不是标准 GZip 格式，尝试强制解压");
            }

            // GZip 解压
            byte[] decompressedBytes = await DecompressGZipBytesAsync(decryptedBytes);
            GD.Print($"GZip 解压完成，解压后字节长度：{decompressedBytes.Length}");

            // 转换为文本（GB18030 编码，带容错）
            string decodedText = GetTextFromBytesSafe(decompressedBytes);

            // === 调试 ===
            // 输出结果
            
            
            GD.Print("解密解压成功！\n文本内容预览：");
            string previewText = decodedText.Substring(0, Math.Min(500, decodedText.Length));
            GD.Print(previewText);

            // 保存解密后的文本到文件
            string outputPath = Path.Combine(
                Path.GetDirectoryName(OS.GetExecutablePath()) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(file)}_decoded.txt"
                );
            await File.WriteAllTextAsync(outputPath, decodedText, GetEncodingSafe("UTF8"));
            GD.Print($"解密后的文本已保存到：{outputPath}");
            
        }
        catch (Exception ex) {
            GD.PrintErr($"处理失败：{ex.Message}");
            GD.PrintErr($"异常详情：{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 安全获取编码（容错降级）
    /// </summary>
    private Encoding GetEncodingSafe(string encodingName) {
        try {
            return Encoding.GetEncoding(encodingName);
        }
        catch {
            GD.PushWarning($"无法获取编码 {encodingName}，降级为 UTF8");
            return Encoding.UTF8;
        }
    }

    /// <summary>
    /// 安全将字节转为文本（多编码尝试）
    /// </summary>
    private string GetTextFromBytesSafe(byte[] bytes) {
        // 优先尝试 GB18030
        try {
            return GetEncodingSafe("GB18030").GetString(bytes);
        }
        catch {
            // 降级为 UTF8
            GD.PushWarning("GB18030 解码失败，尝试 UTF8 解码");
            return Encoding.UTF8.GetString(bytes);
        }
    }

    /// <summary>
    /// 验证字节流是否为标准GZip格式（检查魔数）
    /// </summary>
    private bool IsValidGZipFormat(byte[] bytes) {
        if (bytes == null || bytes.Length < 2)
            return false;
        
        return bytes[0] == GZipMagicNumber[0] && bytes[1] == GZipMagicNumber[1];
    }

    /// <summary>
    /// 异步解压GZip字节数组（兼容非标准GZip格式）
    /// </summary>
    private async Task<byte[]> DecompressGZipBytesAsync(byte[] compressedBytes) {
        if (compressedBytes == null || compressedBytes.Length == 0)
            throw new ArgumentException("压缩字节数组不能为空", nameof(compressedBytes));

        using var inputStream = new MemoryStream(compressedBytes);
        await using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        // 异步拷贝（增加超时容错）
        await gzipStream.CopyToAsync(outputStream);
        return outputStream.ToArray();
    }
}